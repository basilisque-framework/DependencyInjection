/*
   Copyright 2023-2025 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace Basilisque.DependencyInjection.CodeAnalysis;

internal static class RegistrationInfoBuilder
{
    internal static IEnumerable<ServiceRegistrationInfo> GetRegistrationInfos(GeneratorSyntaxContext context, INamedTypeSymbol baseAttrInterface, INamedTypeSymbol nodeSymbol, Microsoft.CodeAnalysis.SyntaxNode? rootNode)
    {
        var result = getRegistrationInfos(context, baseAttrInterface, nodeSymbol, rootNode);

        foreach (var item in result)
        {
            if (!ValidateRegistrationInfo(item))
                continue;

            resolveFactoryInformation(context, item);

            yield return item;
        }
    }

    internal static bool ValidateRegistrationInfo(ServiceRegistrationInfo? registrationInfo)
    {
        if (registrationInfo == null)
            return false;

        if (registrationInfo.ImplementationSymbol == null)
            return false;

        if (registrationInfo.ImplementationSyntaxNode == null)
            return false;

        if (registrationInfo.RegistrationScope == null)
            return false;

        return true;
    }

    private static IEnumerable<ServiceRegistrationInfo> getRegistrationInfos(GeneratorSyntaxContext context, INamedTypeSymbol baseAttrInterface, INamedTypeSymbol nodeSymbol, Microsoft.CodeAnalysis.SyntaxNode? rootNode)
    {
        var isRootNode = rootNode is not null;

        if (isRootNode && context.SemanticModel.Compilation.HasImplicitConversion(nodeSymbol, baseAttrInterface))
            yield break;

        var registrationAttributes = nodeSymbol.GetAttributes()
            .Where(a => context.SemanticModel.Compilation.HasImplicitConversion(a.AttributeClass, baseAttrInterface));

        var interfaceRegistrationAttributes = nodeSymbol.AllInterfaces.SelectMany(i => i.GetAttributes())
            .Where(a => context.SemanticModel.Compilation.HasImplicitConversion(a.AttributeClass, baseAttrInterface));

        registrationAttributes = registrationAttributes.Union(interfaceRegistrationAttributes);

        if (!registrationAttributes.Any())
            yield break;

        foreach (var registrationAttribute in registrationAttributes)
        {
            if (registrationAttribute.AttributeClass is null)
                continue;

            var childRegistrationInfos = getRegistrationInfos(context, baseAttrInterface, registrationAttribute.AttributeClass, null);

            var args = readAttributeArguments(context, nodeSymbol, registrationAttribute);

            var servicesToRegister = args.servicesToRegister;

            if (isRootNode && args.implementsITypeName)
                checkImplementsITypeName(nodeSymbol, ref servicesToRegister);

            bool isHandled = false;
            foreach (var childRegistrationInfo in childRegistrationInfos)
            {
                isHandled = true;

                assignValuesToRegistrationInfo(childRegistrationInfo, rootNode, nodeSymbol, args.registrationScope, servicesToRegister, args.factoryType, args.factoryMethodName, args.serviceKey);

                yield return childRegistrationInfo;
            }

            if (!isHandled)
            {
                var result = new ServiceRegistrationInfo();

                assignValuesToRegistrationInfo(result, rootNode, nodeSymbol, args.registrationScope, servicesToRegister, args.factoryType, args.factoryMethodName, args.serviceKey);

                yield return result;
            }
        }
    }

    private static void checkImplementsITypeName(INamedTypeSymbol nodeSymbol, ref List<INamedTypeSymbol>? servicesToRegister)
    {
        string targetInterfaceName = $"I{nodeSymbol.Name}";
        var implementedITypeNameInterfaces = nodeSymbol.AllInterfaces.Where(i => i.Name == targetInterfaceName);

        if (!implementedITypeNameInterfaces.Any())
            return;

        if (servicesToRegister == null)
            servicesToRegister = new List<INamedTypeSymbol>();

        foreach (var item in implementedITypeNameInterfaces)
        {
            var itemToRegister = item;

            if (item.IsGenericType && !item.IsUnboundGenericType)
            {
                var hasTypeArgumentsWithRealTypes = item.TypeArguments.Any() && !item.TypeArguments.Any(a => a.Kind != SymbolKind.TypeParameter);

                if (hasTypeArgumentsWithRealTypes)
                    itemToRegister = item.ConstructUnboundGenericType();
            }

            if (!servicesToRegister.Contains(itemToRegister))
                servicesToRegister.Add(itemToRegister);
        }
    }

    private static (Registration.Annotations.RegistrationScope? registrationScope, List<INamedTypeSymbol>? servicesToRegister, bool implementsITypeName, INamedTypeSymbol? factoryType, string? factoryMethodName, string? serviceKey) readAttributeArguments(GeneratorSyntaxContext context, INamedTypeSymbol typeToRegister, AttributeData registrationAttribute)
    {
        Registration.Annotations.RegistrationScope? registrationScope = null;
        List<INamedTypeSymbol>? servicesToRegister = null;
        bool implementsITypeName = true;
        INamedTypeSymbol? factoryType = null;
        string? factoryMethodName = null;
        string? serviceKey = null;

        foreach (var ctorArg in registrationAttribute.ConstructorArguments)
        {
            if (ctorArg.Type?.ToDisplayString() == typeof(Registration.Annotations.RegistrationScope).FullName)
            {
                if (System.Enum.TryParse(ctorArg.Value?.ToString(), out Registration.Annotations.RegistrationScope innerRegistrationScope))
                    registrationScope = innerRegistrationScope;
            }
        }

        foreach (var namedArgument in registrationAttribute.NamedArguments)
        {
            if (namedArgument.Value.Kind == TypedConstantKind.Enum && (namedArgument.Key == "Scope" || namedArgument.Key == "RegistrationScope"))
            {
                if (System.Enum.TryParse(namedArgument.Value.Value?.ToString(), out Registration.Annotations.RegistrationScope innerRegistrationScope))
                    registrationScope = innerRegistrationScope;
            }
            else if (namedArgument.Value.Kind == TypedConstantKind.Type && (namedArgument.Key == "As" || namedArgument.Key == "RegisterAs"))
            {
                var innerServiceToRegister = namedArgument.Value.Value as INamedTypeSymbol;

                if (innerServiceToRegister is null)
                    continue;

                servicesToRegister = new List<INamedTypeSymbol>() { innerServiceToRegister };
            }
            else if (namedArgument.Value.Kind == TypedConstantKind.Primitive && (namedArgument.Key == "ImplementsITypeName"))
            {
                bool innerImplementsITypeName;
                if (bool.TryParse(namedArgument.Value.Value?.ToString(), out innerImplementsITypeName))
                    implementsITypeName = innerImplementsITypeName;
            }
            else if (namedArgument.Value.Kind == TypedConstantKind.Type && (namedArgument.Key == "Factory"))
            {
                if (namedArgument.Value.Value is null)
                    continue;

                factoryType = (INamedTypeSymbol)namedArgument.Value.Value;
            }
            else if (namedArgument.Value.Kind == TypedConstantKind.Primitive && (namedArgument.Key == "FactoryMethodName"))
            {
                var tmpFactoryMethodName = namedArgument.Value.Value as string;

                if (string.IsNullOrWhiteSpace(tmpFactoryMethodName))
                    continue;

                factoryMethodName = tmpFactoryMethodName;
            }
            else if (namedArgument.Key == "Key")
            {
                serviceKey = getServiceKeyFromNamedArgument(context, registrationAttribute, namedArgument);
            }
        }

        return (registrationScope, servicesToRegister, implementsITypeName, factoryType, factoryMethodName, serviceKey);
    }

    private static string? getServiceKeyFromNamedArgument(GeneratorSyntaxContext context, AttributeData registrationAttribute, KeyValuePair<string, TypedConstant> namedArgument)
    {
        var attributeSyntax = registrationAttribute.ApplicationSyntaxReference?.GetSyntax() as Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax;

        var keyArgumentSyntax = attributeSyntax?.ArgumentList?.Arguments.Where(a => a.NameEquals?.Name.Identifier.Text == "Key").SingleOrDefault();

        var expression = keyArgumentSyntax?.Expression;

        // This sould never be null in that this case in user code that compiles successfully.
        if (expression is null)
            return null;

        if (namedArgument.Value.Kind == TypedConstantKind.Primitive && namedArgument.Value.Value is string s && expression.ToString().StartsWith("nameof("))
            return $"\"{s}\"";

        var rewriter = new FullQualifyingSyntaxRewriter(context.SemanticModel);

        return rewriter.Visit(expression).ToFullString();
    }

    private static void assignValuesToRegistrationInfo(ServiceRegistrationInfo registrationInfo, Microsoft.CodeAnalysis.SyntaxNode? implementationNode, INamedTypeSymbol implementationNodeSymbol, Registration.Annotations.RegistrationScope? registrationScope, List<INamedTypeSymbol>? servicesToRegister, INamedTypeSymbol? factoryType, string? factoryMethodName, string? serviceKey)
    {
        if (implementationNode != null)
        {
            registrationInfo.ImplementationSyntaxNode = implementationNode;
            registrationInfo.ImplementationSymbol = implementationNodeSymbol;
        }

        if (registrationScope != null)
            registrationInfo.RegistrationScope = registrationScope;

        if (servicesToRegister != null)
        {
            foreach (var serviceToRegister in servicesToRegister)
            {
                registrationInfo.RegisteredServices.Add(serviceToRegister);
            }
        }

        if (factoryType is not null)
            registrationInfo.FactoryType = factoryType;

        if (factoryMethodName is not null)
            registrationInfo.FactoryMethodName = factoryMethodName;

        if (serviceKey is not null)
            registrationInfo.ServiceKey = serviceKey;
    }

    private static void resolveFactoryInformation(GeneratorSyntaxContext context, ServiceRegistrationInfo item)
    {
        if (item.FactoryType is null)
        {
            if (item.FactoryMethodName is not null)
            {
                // The factory method name is defined, but no factory type is defined.
                // This doesn't make sense, so we report a diagnostic.

                var location = item.ImplementationSyntaxNode?.GetLocation() ?? Location.None;
                var factoryTypeDiagnostic = Diagnostic.Create(DiagnosticDescriptors.FactoryTypeNotDefined, location, item.FactoryMethodName);
                item.Diagnostics.Add(factoryTypeDiagnostic);
            }

            // No factory type or factory method name defined.
            // This is fine, so we just register the service without a factory.
            return;
        }

        var factoryTypeName = item.FactoryType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        bool isKeyedRegistration = item.ServiceKey is not null;

        var factoryMethod = getValidFactoryMethod(context, item.FactoryType, isKeyedRegistration, item.FactoryMethodName);
        if (factoryMethod is null)
        {
            // No valid factory method found, so we report a diagnostic.
            var location = item.ImplementationSyntaxNode?.GetLocation() ?? Location.None;
            Diagnostic factoryMethodDiagnostic;
            if (item.FactoryMethodName is null)
                factoryMethodDiagnostic = Diagnostic.Create(DiagnosticDescriptors.FactoryMethodNotFound, location, factoryTypeName);
            else
                factoryMethodDiagnostic = Diagnostic.Create(DiagnosticDescriptors.FactoryMethodNameIsInvalid, location, item.FactoryMethodName, factoryTypeName);
            item.Diagnostics.Add(factoryMethodDiagnostic);
            return;
        }

        item.FactoryRegistrationWithoutImplementation = !context.SemanticModel.Compilation.HasImplicitConversion(factoryMethod.ReturnType, item.ImplementationSymbol);

        item.FactoryInformation = $"{factoryTypeName}.{factoryMethod.Name}";
    }

    private static IMethodSymbol? getValidFactoryMethod(GeneratorSyntaxContext context, INamedTypeSymbol factoryType, bool isKeyed, string? expectedMethodName)
    {
        var getMembers = expectedMethodName is null ? factoryType.GetMembers() : factoryType.GetMembers(expectedMethodName);

        var methodCandidates = getMembers.OfType<IMethodSymbol>().Where(member =>
        {
            // has to be static
            if (!member.IsStatic)
                return false;

            // has to be public or internal
            if (member.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
                return false;

            // has to return a value (not void)
            if (member.ReturnsVoid)
                return false;

            // regular methods only (no constructors, operators, property accessors, ...)
            if (member.MethodKind != MethodKind.Ordinary)
                return false;

            // check parameters
            var parameters = member.Parameters;

            if (isKeyed)
            {
                if (parameters.Length == 2 &&
                     parameters[0].Type.ToDisplayString() == "System.IServiceProvider" &&
                     parameters[1].Type.ToDisplayString() is "object" or "object?")
                {
                    // Keyed Factory
                    return true;
                }
            }
            else
            {
                if (parameters.Length == 1 &&
                parameters[0].Type.ToDisplayString() == "System.IServiceProvider")
                {
                    // Non-Keyed Factory
                    return true;
                }
            }

            return false;
        });

        try
        {
            var factoryMethod = methodCandidates.SingleOrDefault();

            return factoryMethod;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
