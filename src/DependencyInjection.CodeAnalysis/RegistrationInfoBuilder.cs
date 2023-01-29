using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    internal static class RegistrationInfoBuilder
    {
        internal static IEnumerable<ServiceRegistrationInfo> GetRegistrationInfos(GeneratorSyntaxContext context, INamedTypeSymbol baseAttrInterface, INamedTypeSymbol nodeSymbol, Microsoft.CodeAnalysis.SyntaxNode? rootNode)
        {
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

                Registration.Annotations.RegistrationScope? registrationScope;
                List<INamedTypeSymbol>? servicesToRegister;
                bool implementsITypeName;
                readAttributeArguments(registrationAttribute, out registrationScope, out servicesToRegister, out implementsITypeName);

                if (implementsITypeName && rootNode != null)
                    checkImplementsITypeName(nodeSymbol, ref servicesToRegister);

                bool isHandled = false;
                foreach (var childRegistrationInfo in childRegistrationInfos)
                {
                    isHandled = true;

                    assignValuesToRegistrationInfo(childRegistrationInfo, rootNode, nodeSymbol, registrationScope, servicesToRegister);

                    yield return childRegistrationInfo;
                }

                if (!isHandled)
                {
                    var result = new ServiceRegistrationInfo();

                    assignValuesToRegistrationInfo(result, rootNode, nodeSymbol, registrationScope, servicesToRegister);

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

        private static void readAttributeArguments(AttributeData registrationAttribute, out Registration.Annotations.RegistrationScope? registrationScope, out List<INamedTypeSymbol>? servicesToRegister, out bool implementsITypeName)
        {
            registrationScope = null;
            servicesToRegister = null;
            implementsITypeName = true;

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
            }
        }

        private static void assignValuesToRegistrationInfo(ServiceRegistrationInfo registrationInfo, Microsoft.CodeAnalysis.SyntaxNode? implementationNode, INamedTypeSymbol implementationNodeSymbol, Registration.Annotations.RegistrationScope? registrationScope, List<INamedTypeSymbol>? servicesToRegister)
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
        }
    }
}
