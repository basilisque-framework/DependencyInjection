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
            if (registrationAttribute.AttributeClass == null)
                continue;

            var childRegistrationInfos = GetRegistrationInfos(context, baseAttrInterface, registrationAttribute.AttributeClass, null);

            var args = readAttributeArguments(context, registrationAttribute);

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
            if (!servicesToRegister.Contains(item))
                servicesToRegister.Add(item);
        }
    }

    private static (Registration.Annotations.RegistrationScope? registrationScope, List<INamedTypeSymbol>? servicesToRegister, bool implementsITypeName, INamedTypeSymbol? factoryType, string? factoryMethodName, string? serviceKey) readAttributeArguments(GeneratorSyntaxContext context, AttributeData registrationAttribute)
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
                if (innerServiceToRegister != null)
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

        if (namedArgument.Value.Kind == TypedConstantKind.Primitive || namedArgument.Value.Kind == TypedConstantKind.Type)
        {
            var result = expression.ToString();

            switch (namedArgument.Value.Value)
            {
                case string s when result.StartsWith("nameof("):
                    return $"\"{s}\"";
                default:
                    return result;
            }
        }
        else if (namedArgument.Value.Kind == TypedConstantKind.Enum)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(expression);
            var symbol = symbolInfo.Symbol;

            // Fallback, in case the symbol is not found, use the first candidate symbol.
            symbol ??= symbolInfo.CandidateSymbols.FirstOrDefault();

            if (symbol is null)
                return null;

            // Fallback in case the symbol is not a field symbol.
            if (symbol is not IFieldSymbol fieldSymbol)
            {
                var enumType = namedArgument.Value.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var enumValue = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                return $"{enumType}.{enumValue}";
            }

            return $"{fieldSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{fieldSymbol.Name}";
        }
        else if (namedArgument.Value.Kind == TypedConstantKind.Array)
        {
            return expression.ToString();
        }

        return null;
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
}
