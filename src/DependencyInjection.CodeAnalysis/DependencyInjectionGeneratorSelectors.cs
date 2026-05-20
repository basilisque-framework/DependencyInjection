/*
   Copyright 2023-2026 Alexander Stärk

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
using Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Common;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    internal static class DependencyInjectionGeneratorSelectors
    {
        private static readonly List<string> _assemblyNamePrefixesToIgnore = new()
        {
            "System",
            "Microsoft",
            "mscorlib",
            "netstandard",
            "NuGet",
            "testhost",
            "WindowsBase"
        };

        private static readonly char[] _dependencyInjectionExtensionsSeparators = new char[] { ';', ',' };

        internal static IEnumerable<string>? DependencyInjectionExtensionsSelector(AnalyzerConfigOptionsProvider provider, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
            cancellationToken.ThrowIfCancellationRequested();

            var relevantReferencedAssemblies = compilation.SourceModule.ReferencedAssemblySymbols.Where(a => !a.IsImplicitlyDeclared && !_assemblyNamePrefixesToIgnore.Any(p => a.Name.StartsWith(p)));

            var referencedDependencyRegistrators = relevantReferencedAssemblies.Select((assembly) => assembly.GetTypeByMetadataName($"{assembly.Name.ToValidNamespace()}.{DependencyInjectionGeneratorData.C_DEPENDENCY_REGISTRATOR_CLASSNAME}"));

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
            cancellationToken.ThrowIfCancellationRequested();

            string? name;
            Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax? baseList;
            SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>? attributes;
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
