using Basilisque.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    [Generator]
    public class DependencyInjectionGenerator : IIncrementalGenerator
    {
        private const string C_DEPENDENCY_REGISTRATOR_CLASSNAME = "DependencyRegistrator";
        private const string C_DEPENDENCY_REGISTRATOR_ROOTNAMESPACE_COMPILATIONNAME = C_DEPENDENCY_REGISTRATOR_CLASSNAME + "_RootNamespace";
        private const string C_DEPENDENCY_REGISTRATOR_GLOBALNAMESPACE_COMPILATIONNAME = C_DEPENDENCY_REGISTRATOR_CLASSNAME + "_GlobalNamespace";

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

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }

            var rootNamespaceProvider = context.AnalyzerConfigOptionsProvider.Select(rootNamespaceSelector);

            var assemblyNameProvider = context.CompilationProvider.Select(static (cmp, ct) => cmp.AssemblyName);

            var referencedAssemblySymbolsProvider = context.CompilationProvider.Select(referencedAssemblySymbolsSelector);

            var combinedValueProvider = rootNamespaceProvider
                .Combine(assemblyNameProvider)
                .Combine(referencedAssemblySymbolsProvider);

            context.RegisterCompilationInfoOutput(combinedValueProvider, outputDependencyRegistratorSource);
        }

        private static string? rootNamespaceSelector(AnalyzerConfigOptionsProvider provider, CancellationToken cancellationToken)
        {
            if (provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? rootNamespace))
                return rootNamespace;
            else
                return null;
        }

        private static IEnumerable<INamedTypeSymbol?> referencedAssemblySymbolsSelector(Compilation compilation, CancellationToken cancellationToken)
        {
            var relevantReferencedAssemblies = compilation.SourceModule.ReferencedAssemblySymbols.Where(a => !_assemblyNamePrefixesToIgnore.Any(p => a.Name.StartsWith(p)));

            var referencedDependencyRegistrators = relevantReferencedAssemblies.Select((assembly) => assembly.GetTypeByMetadataName($"{assembly.Name}.{C_DEPENDENCY_REGISTRATOR_CLASSNAME}"));

            var result = referencedDependencyRegistrators.Where(namedTypeSymbol => namedTypeSymbol != null);

            return result;
        }

        private static void outputDependencyRegistratorSource(SourceProductionContext context, ((string? RootNamespace, string? AssemblyName) Options, IEnumerable<INamedTypeSymbol?> NamedTypes) provider, Language lang, CompilationInfoFactory newCompilationInfo)
        {
            if (!hasRootNamespace(context, provider.Options.RootNamespace))
                return;

            if (lang != Language.CSharp)
                throw new System.NotSupportedException($"The language '{lang}' is currently not supported by this generator.");

            string rootNamespace = provider.Options.RootNamespace!; //Due to usage of netstandard2.0 for source generators, the method hasRootNamespace cannot specify the NotNullWhen attribute. Therefore we need the !

            newCompilationInfo(C_DEPENDENCY_REGISTRATOR_ROOTNAMESPACE_COMPILATIONNAME, rootNamespace)
                .AddNewClassInfo(C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
                {
                })
                .AddToSourceProductionContext(context, lang);


            if (!string.IsNullOrEmpty(provider.Options.AssemblyName) && rootNamespace != provider.Options.AssemblyName)
            {
                var ns = provider.Options.AssemblyName.ToValidNamespace();

                newCompilationInfo(C_DEPENDENCY_REGISTRATOR_GLOBALNAMESPACE_COMPILATIONNAME, targetNamespace: ns)
                    .AddNewClassInfo(C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
                    {
                        cl.BaseClass = rootNamespace + "." + C_DEPENDENCY_REGISTRATOR_CLASSNAME;
                    })
                    .AddToSourceProductionContext(context, lang);
            }
        }

        private static bool hasRootNamespace(SourceProductionContext context, /*[System.Diagnostics.CodeAnalysis.NotNullWhen(true)]*/ string? rootNamespace)
        {
            if (string.IsNullOrWhiteSpace(rootNamespace))
            {
                var missingRootNamespaceDiagnostic = Diagnostic.Create(new DiagnosticDescriptor("BAS-DI-001", "An assembly should always have a root namespace defined.", $"The assembly has no root namespace defined.", "Basilisque.DependencyInjection", DiagnosticSeverity.Error, true), Location.None);
                context.ReportDiagnostic(missingRootNamespaceDiagnostic);
                return false;
            }

            return true;
        }

    }



    //            var referencedAssemblies = context.Compilation.SourceModule.ReferencedAssemblySymbols.Where(a => !_assemblyNamePrefixesToIgnore.Any(p => a.Name.StartsWith(p)));

    //            var types = referencedAssemblies.SelectMany(a =>
    //            {
    //                try
    //                {
    //                    var main = a.Identity.Name.Split('.').Aggregate(a.GlobalNamespace, (s, c) =>
    //                    {
    //                        return s.GetNamespaceMembers().Single(m => m.Name.Equals(c));
    //                    });

    //                    return getAllTypes(main);
    //                }
    //                catch
    //                {
    //                    return Enumerable.Empty<ITypeSymbol>();
    //                }
    //            });




    //            //            // Find the main method
    //            //            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

    //            //            // Build up the source code
    //            //            string source = $@" // Auto-generated code
    //            //using System;

    //            //namespace {mainMethod.ContainingNamespace.ToDisplayString()}
    //            //{{
    //            //    public static partial class {mainMethod.ContainingType.Name}
    //            //    {{
    //            //        static partial void HelloFrom(string name) =>
    //            //            Console.WriteLine($""Generator says: Hi from '{{name}}'"");
    //            //    }}
    //            //}}
    //            //";
    //            //            var typeName = mainMethod.ContainingType.Name;

    //            //            // Add the source code to the compilation
    //            //            context.AddSource($"{typeName}.g.cs", source);
    //            var test = types.ToList();
    //        }

    //        private static IEnumerable<ITypeSymbol> getAllTypes(INamespaceSymbol root)
    //        {
    //            foreach (var namespaceOrTypeSymbol in root.GetMembers())
    //            {
    //                if (namespaceOrTypeSymbol is INamespaceSymbol @namespace) foreach (var nested in getAllTypes(@namespace)) yield return nested;

    //                else if (namespaceOrTypeSymbol is ITypeSymbol type) yield return type;
    //            }
    //        }
    //    }
}
