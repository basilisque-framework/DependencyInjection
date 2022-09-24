using Basilisque.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    internal static class DependencyInjectionGeneratorSelectors
    {
        internal const string C_DEPENDENCY_REGISTRATOR_CLASSNAME = "DependencyRegistrator";

        private static List<string> _assemblyNamePrefixesToIgnore = new List<string>()
        {
            "System",
            "Microsoft",
            "mscorlib",
            "netstandard",
            "NuGet",
            "testhost",
            "WindowsBase"
        };

        internal static string? RootNamespaceSelector(AnalyzerConfigOptionsProvider provider, CancellationToken cancellationToken)
        {
            if (provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? rootNamespace))
            {
                //#if DEBUG
                //                if (!System.Diagnostics.Debugger.IsAttached && rootNamespace == "Basilisque.DependencyInjection.Tests")
                //                {
                //                    System.Diagnostics.Debugger.Launch();
                //                }
                //#endif

                return rootNamespace;
            }
            else
                return null;
        }

        internal static IEnumerable<INamedTypeSymbol> ReferencedAssemblySymbolsSelector(Compilation compilation, CancellationToken cancellationToken)
        {
            var relevantReferencedAssemblies = compilation.SourceModule.ReferencedAssemblySymbols.Where(a => !a.IsImplicitlyDeclared && !_assemblyNamePrefixesToIgnore.Any(p => a.Name.StartsWith(p)));

            var referencedDependencyRegistrators = relevantReferencedAssemblies.Select((assembly) => assembly.GetTypeByMetadataName($"{assembly.Name.ToValidNamespace()}.{C_DEPENDENCY_REGISTRATOR_CLASSNAME}"));

            var result = referencedDependencyRegistrators.Where(namedTypeSymbol => namedTypeSymbol != null);

            return result!;
        }

        internal static IncrementalValueProvider<ImmutableArray<List<ServiceRegistrationInfo>?>> CreateServicesToRegisterValueProvider(IncrementalGeneratorInitializationContext context)
        {
            var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
                static (s, ct) => isSyntaxTargetForGeneration(s, ct),
                static (ctx, ct) => getSemanticTargetForGeneration(ctx, ct)
                ).Where(static m => m is not null);

            return syntaxProvider.Collect();
        }

        private static bool isSyntaxTargetForGeneration(Microsoft.CodeAnalysis.SyntaxNode node, CancellationToken cancellationToken)
        {
            string? name = null;
            Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax? baseList = null;
            SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>? attributes = null;
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax cds)
            {
                name = cds.Identifier.ValueText;
                baseList = cds.BaseList;
                attributes = cds.AttributeLists;
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax sds)
            {
                name = sds.Identifier.ValueText;
                baseList = sds.BaseList;
                attributes = sds.AttributeLists;
            }
            else
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(name) && (baseList?.Types.Count > 0 || attributes?.Count > 0);
        }

        private static List<ServiceRegistrationInfo>? getSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var nodeSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) as INamedTypeSymbol;

            if (nodeSymbol == null)
                return null;

            if (nodeSymbol.IsAbstract)
                return null;

            if (string.IsNullOrWhiteSpace(nodeSymbol.Name))
                return null;

            var baseAttrInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("Basilisque.DependencyInjection.Registration.Annotations.IRegisterServiceAttribute");
            if (baseAttrInterface == null)
                return null;

            var registrationInfos = getRegistrationInfos(context, baseAttrInterface, nodeSymbol, context.Node);

            var result = registrationInfos.Select(r =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return r;
            }).ToList();

            return result;
        }

        private static IEnumerable<ServiceRegistrationInfo> getRegistrationInfos(GeneratorSyntaxContext context, INamedTypeSymbol baseAttrInterface, INamedTypeSymbol nodeSymbol, Microsoft.CodeAnalysis.SyntaxNode? rootNode)
        {
            var registrationAttributes = nodeSymbol.GetAttributes()
                .Where(a => context.SemanticModel.Compilation.HasImplicitConversion(a.AttributeClass, baseAttrInterface));

            if (!registrationAttributes.Any())
                yield break;

            foreach (var registrationAttribute in registrationAttributes)
            {
                if (registrationAttribute.AttributeClass == null)
                    continue;

                var childRegistrationInfos = getRegistrationInfos(context, baseAttrInterface, registrationAttribute.AttributeClass, null);

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
