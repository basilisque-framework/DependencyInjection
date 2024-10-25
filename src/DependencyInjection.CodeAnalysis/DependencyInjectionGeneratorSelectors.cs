/*
   Copyright 2023 Alexander Stärk

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

using Basilisque.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
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

        private static char[] _dependencyInjectionExtensionsSeparators = new char[] { ';', ',' };

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

        internal static IEnumerable<string>? DependencyInjectionExtensionsSelector(AnalyzerConfigOptionsProvider provider, CancellationToken cancellationToken)
        {
            if (provider.GlobalOptions.TryGetValue("build_property.BAS_DI_Extensions", out string? dependencyInjectionExtensions)
                && !string.IsNullOrWhiteSpace(dependencyInjectionExtensions))
            {
                var extensions = dependencyInjectionExtensions.Split(_dependencyInjectionExtensionsSeparators, System.StringSplitOptions.RemoveEmptyEntries);

                return extensions;
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
                predicate: static (s, ct) => isSyntaxTargetForGeneration(s, ct),
                transform: static (ctx, ct) => getSemanticTargetForGeneration(ctx, ct)
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

            var registrationInfos = RegistrationInfoBuilder.GetRegistrationInfos(context, baseAttrInterface, nodeSymbol, context.Node);

            var result = registrationInfos.Select(r =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return r;
            }).ToList();

            return result;
        }
    }
}
