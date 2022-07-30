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
        private const string C_DEPENDENCY_REGISTRATOR_ASSEMBLYNAMENAMESPACE_COMPILATIONNAME = C_DEPENDENCY_REGISTRATOR_CLASSNAME + "_AssemblyNameNamespace";
        private const string C_DEPENDENCY_REGISTRATOR_XMLCOMMENT_DESCRIPTION = @"Registers all dependencies and services of this assembly.";
        private const string C_DEPENDENCYREGISTRATORBUILDER_TYPE = "Basilisque.DependencyInjection.Registration.DependencyRegistratorBuilder";
        private const string C_IDEPENDENCYREGISTRATOR_TYPE = "Basilisque.DependencyInjection.Registration.IDependencyRegistrator";

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
            var rootNamespaceProvider = context.AnalyzerConfigOptionsProvider.Select(rootNamespaceSelector);

            var assemblyNameProvider = context.CompilationProvider.Select(static (cmp, ct) => cmp.AssemblyName);

            var referencedAssemblySymbolsProvider = context.CompilationProvider.Select(referencedAssemblySymbolsSelector);

            var combinedValueProviderStub = rootNamespaceProvider.Combine(assemblyNameProvider);

            var combinedValueProviderImpl = combinedValueProviderStub.Combine(referencedAssemblySymbolsProvider);

            context.RegisterCompilationInfoOutput(combinedValueProviderStub, outputStubs);
            context.RegisterImplementationCompilationInfoOutput(combinedValueProviderImpl, outputImplementations);
        }

        private static string? rootNamespaceSelector(AnalyzerConfigOptionsProvider provider, CancellationToken cancellationToken)
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

        private static IEnumerable<INamedTypeSymbol> referencedAssemblySymbolsSelector(Compilation compilation, CancellationToken cancellationToken)
        {
            var relevantReferencedAssemblies = compilation.SourceModule.ReferencedAssemblySymbols.Where(a => !a.IsImplicitlyDeclared && !_assemblyNamePrefixesToIgnore.Any(p => a.Name.StartsWith(p)));

            var referencedDependencyRegistrators = relevantReferencedAssemblies.Select((assembly) => assembly.GetTypeByMetadataName($"{assembly.Name.ToValidNamespace()}.{C_DEPENDENCY_REGISTRATOR_CLASSNAME}"));

            var result = referencedDependencyRegistrators.Where(namedTypeSymbol => namedTypeSymbol != null);

            return result!;
        }

        private static bool checkPreconditions(SourceProductionContext context, (string? RootNamespace, string? AssemblyName) provider, RegistrationOptions registrationOptions)
        {
            //check preconditions
            if (registrationOptions.Language != Language.CSharp)
                throw new System.NotSupportedException($"The language '{registrationOptions.Language}' is currently not supported by this generator.");

            if (!checkPreconditionAssemblyName(context, provider.AssemblyName))
                return false;

            return true;
        }

        private static bool checkPreconditionAssemblyName(SourceProductionContext context, string? assemblyName)
        {
            if (!string.IsNullOrEmpty(assemblyName))
                return true;

            var missingAssemblyNameDiagnostic = Diagnostic.Create(DiagnosticDescriptors.MissingAssemblyName, Location.None);
            context.ReportDiagnostic(missingAssemblyNameDiagnostic);

            return false;
        }

        private static (string mainCompilationName, string mainNamespace, bool hasRootNamespace, string? rootNamespace, string assemblyNameNamespace) getMainCompilationTarget((string? RootNamespace, string? AssemblyName) provider)
        {
            //get the target namespace(s)
            var rootNamespace = provider.RootNamespace;
            var hasRootNamespace = !string.IsNullOrWhiteSpace(rootNamespace) && rootNamespace != provider.AssemblyName;

            var assemblyNameNamespace = provider.AssemblyName.ToValidNamespace()!;

            //check if the target project has a rootnamespace defined and if it differs from the assemblyname
            string mainCompilationName;
            string mainNamespace;
            if (hasRootNamespace)
            {
                //prepare to output the main registrator class in the rootnamespace
                mainCompilationName = C_DEPENDENCY_REGISTRATOR_ROOTNAMESPACE_COMPILATIONNAME;
                mainNamespace = rootNamespace!;
            }
            else
            {
                //prepare to output the main registrator class in the assemblyname as namespace
                mainCompilationName = C_DEPENDENCY_REGISTRATOR_ASSEMBLYNAMENAMESPACE_COMPILATIONNAME;
                mainNamespace = assemblyNameNamespace;
            }

            return (mainCompilationName, mainNamespace, hasRootNamespace, rootNamespace, assemblyNameNamespace);
        }

        private static void outputStubs(SourceProductionContext context, (string? RootNamespace, string? AssemblyName) provider, RegistrationOptions registrationOptions)
        {
            if (!checkPreconditions(context, provider, registrationOptions))
                return;

            (string mainCompilationName, string mainNamespace, bool hasRootNamespace, string? rootNamespace, string assemblyNameNamespace) = getMainCompilationTarget(provider);

            //output the source
            outputDependencyRegistratorStub(registrationOptions, hasRootNamespace, rootNamespace, mainNamespace, mainCompilationName, assemblyNameNamespace);
            outputServiceCollectionExtensionMethods(registrationOptions, mainNamespace);
        }

        private static void outputImplementations(SourceProductionContext context, ((string? RootNamespace, string? AssemblyName) Options, IEnumerable<INamedTypeSymbol> NamedDependencyRegistratorTypes) provider, RegistrationOptions registrationOptions)
        {
            if (!checkPreconditions(context, provider.Options, registrationOptions))
                return;

            (string mainCompilationName, string mainNamespace, _, _, _) = getMainCompilationTarget(provider.Options);

            outputDependencyRegistratorImplementation(context.CancellationToken, registrationOptions, mainNamespace, mainCompilationName, provider.NamedDependencyRegistratorTypes);
        }

        private static void outputDependencyRegistratorStub(RegistrationOptions registrationOptions, bool hasRootNamespace, string? rootNamespace, string mainNamespace, string mainCompilationName, string assemblyNameNamespace)
        {

            if (hasRootNamespace)
            {
                //output a helper class in the assemblyname as namespace to make it easier to find in completely compiled assemblies
                registrationOptions.CreateCompilationInfo(C_DEPENDENCY_REGISTRATOR_ASSEMBLYNAMENAMESPACE_COMPILATIONNAME, targetNamespace: assemblyNameNamespace)
                    .AddNewClassInfo(C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
                    {
                        cl.IsSealed = true;
                        cl.BaseClass = string.Concat(rootNamespace, ".", C_DEPENDENCY_REGISTRATOR_CLASSNAME);

                        cl.XmlDocSummary = $@"{C_DEPENDENCY_REGISTRATOR_XMLCOMMENT_DESCRIPTION}
This class mainly exists for performance and simplicity reasons during code compilation.
Although there is technically no reason to not manually interact with this class, you should probably prefer to use the identical class in your root namespace (<see cref=""{cl.BaseClass}""/>).";

                    })
                    .AddToSourceProductionContext();
            }

            //output the main registrator class
            registrationOptions.CreateCompilationInfo(mainCompilationName, mainNamespace)
                .AddNewClassInfo(C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
                {
                    cl.IsPartial = true;
                    cl.BaseClass = "Basilisque.DependencyInjection.Registration.BaseDependencyRegistrator";
                    cl.XmlDocSummary = C_DEPENDENCY_REGISTRATOR_XMLCOMMENT_DESCRIPTION;

                    var performInitializationMethod = new MethodInfo(AccessModifier.Protected, "void", "PerformInitialization")
                    {
                        IsOverride = true,
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Basilisque.DependencyInjection.Registration.DependencyCollection", "collection")
                        }
                    };
                    performInitializationMethod.Body.Append(@"
doBeforeInitialization(collection);

initializeDependenciesGenerated(collection);

doAfterInitialization(collection);
");
                    cl.Methods.Add(performInitializationMethod);

                    cl.Methods.Add(new MethodInfo(true, "doBeforeInitialization")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Basilisque.DependencyInjection.Registration.DependencyCollection", "collection")
                        }
                    });

                    cl.Methods.Add(new MethodInfo(true, "initializeDependenciesGenerated")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Basilisque.DependencyInjection.Registration.DependencyCollection", "collection")
                        }
                    });

                    cl.Methods.Add(new MethodInfo(true, "doAfterInitialization")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Basilisque.DependencyInjection.Registration.DependencyCollection", "collection")
                        }
                    });


                    var performServiceRegistrationMethod = new MethodInfo(AccessModifier.Protected, "void", "PerformServiceRegistration")
                    {
                        IsOverride = true,
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Microsoft.Extensions.DependencyInjection.IServiceCollection", "services")
                        }
                    };
                    performServiceRegistrationMethod.Body.Append(@"
doBeforeRegistration(services);

registerServicesGenerated(services);

doAfterRegistration(services);
");
                    cl.Methods.Add(performServiceRegistrationMethod);

                    cl.Methods.Add(new MethodInfo(true, "doBeforeRegistration")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Microsoft.Extensions.DependencyInjection.IServiceCollection", "services")
                        }
                    });

                    cl.Methods.Add(new MethodInfo(true, "registerServicesGenerated")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Microsoft.Extensions.DependencyInjection.IServiceCollection", "services")
                        }
                    });

                    cl.Methods.Add(new MethodInfo(true, "doAfterRegistration")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Microsoft.Extensions.DependencyInjection.IServiceCollection", "services")
                        }
                    });
                })
                .AddToSourceProductionContext();
        }

        private static void outputServiceCollectionExtensionMethods(RegistrationOptions registrationOptions, string mainNamespace)
        {
            const string C_ISERVICECOLLECTIONEXTENSIONS_CLASSNAME = "IServiceCollectionExtensions";
            const string C_ISERVICECOLLECTION_TYPE = "Microsoft.Extensions.DependencyInjection.IServiceCollection";

            var dependencyRegistratorFullQualifiedName = $"{mainNamespace}.{C_DEPENDENCY_REGISTRATOR_CLASSNAME}";
            var dependencyRegistratorBuilderNameWithGeneric = $"{C_DEPENDENCYREGISTRATORBUILDER_TYPE}<{dependencyRegistratorFullQualifiedName}>";
            var dependencyRegistratorBuilderNameWithGenericForXmlDoc = $"{C_DEPENDENCYREGISTRATORBUILDER_TYPE}{{TDependencyRegistrator}}";

            registrationOptions.CreateCompilationInfo(C_ISERVICECOLLECTIONEXTENSIONS_CLASSNAME, mainNamespace)
                .AddNewClassInfo(C_ISERVICECOLLECTIONEXTENSIONS_CLASSNAME, cl =>
                {
                    cl.IsStatic = true;
                    cl.XmlDocSummary = $"This class contains extension methods for <see cref=\"{C_ISERVICECOLLECTION_TYPE}\"/>";


                    var initializeDependenciesExtensionMethod = new MethodInfo(AccessModifier.Public, $"{dependencyRegistratorBuilderNameWithGeneric}", "InitializeDependencies")
                    {
                        XmlDocSummary = $@"This method extends <see cref=""{C_ISERVICECOLLECTION_TYPE}""/> with a mechanism to register dependencies and services for the whole application.
Calling this method creates a <see cref=""{dependencyRegistratorBuilderNameWithGenericForXmlDoc}""/> and initializes the dependency chain.",

                        XmlDocAdditionalLines = {
                            $"<param name=\"services\">The <see cref=\"{C_ISERVICECOLLECTION_TYPE}\"/> all services are registered on.</param>",
                            $"<returns>A <see cref=\"{dependencyRegistratorBuilderNameWithGenericForXmlDoc}\"/> that is used to build and execute the chain of <see cref=\"{C_IDEPENDENCYREGISTRATOR_TYPE}\"/></returns>"
                        },

                        IsExtensionMethod = true,

                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, C_ISERVICECOLLECTION_TYPE, "services")
                        }
                    };
                    initializeDependenciesExtensionMethod.Body.Append($@"return Basilisque.DependencyInjection.IServiceCollectionExtensions.InitializeDependencies<{dependencyRegistratorFullQualifiedName}>(services);");
                    cl.Methods.Add(initializeDependenciesExtensionMethod);


                    var registerServicesExtensionMethod = new MethodInfo(AccessModifier.Public, "void", "RegisterServices")
                    {
                        XmlDocSummary = $@"This method extends <see cref=""{C_ISERVICECOLLECTION_TYPE}""/> with a mechanism to register dependencies and services for the whole application.
Calling this method creates a <see cref=""{dependencyRegistratorBuilderNameWithGenericForXmlDoc}""/>, initializes the dependency chain and executes the registration of all services.
For more control over the details of this process use <see cref=""InitializeDependencies""/> instead.",

                        XmlDocAdditionalLines = {
                            $"<param name=\"services\">The <see cref=\"{C_ISERVICECOLLECTION_TYPE}\"/> all services are registered on.</param>"
                        },

                        IsExtensionMethod = true,

                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, C_ISERVICECOLLECTION_TYPE, "services")
                        }
                    };
                    registerServicesExtensionMethod.Body.Append($@"Basilisque.DependencyInjection.IServiceCollectionExtensions.InitializeDependencies<{dependencyRegistratorFullQualifiedName}>(services).RegisterServices();");
                    cl.Methods.Add(registerServicesExtensionMethod);
                })
                .AddToSourceProductionContext();
        }

        private static void outputDependencyRegistratorImplementation(CancellationToken cancellationToken, RegistrationOptions registrationOptions, string mainNamespace, string mainCompilationName, IEnumerable<INamedTypeSymbol> namedDependencyRegistratorTypes)
        {
            //output implementation of the main registrator class
            var ci = registrationOptions.CreateCompilationInfo($"{mainCompilationName}.impl", mainNamespace);
            ci.AddGeneratedCodeAttributes = false;
            ci.AddNewClassInfo(C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
                {
                    cl.IsPartial = true;

                    var initializeDependenciesGeneratedMethod = new MethodInfo(true, "initializeDependenciesGenerated")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Basilisque.DependencyInjection.Registration.DependencyCollection", "collection")
                        }
                    };
                    initializeDependenciesGeneratedMethod.Body.Append(@"/* initialize dependencies - generated from assembly dependencies */");
                    addDependenciesToBody(cancellationToken, initializeDependenciesGeneratedMethod.Body, namedDependencyRegistratorTypes);
                    cl.Methods.Add(initializeDependenciesGeneratedMethod);

                    var registerServicesGeneratedMethod = new MethodInfo(true, "registerServicesGenerated")
                    {
                        Parameters = {
                            new ParameterInfo(ParameterKind.Ordinary, "Microsoft.Extensions.DependencyInjection.IServiceCollection", "services")
                        }
                    };
                    registerServicesGeneratedMethod.Body.Append(@"/* register services - generated from the current project */");
                    addServicesToBody(cancellationToken, registerServicesGeneratedMethod.Body);
                    cl.Methods.Add(registerServicesGeneratedMethod);
                })
                .AddToSourceProductionContext();
        }

        private static void addDependenciesToBody(CancellationToken cancellationToken, CodeLines body, IEnumerable<INamedTypeSymbol> namedDependencyRegistratorTypes)
        {
            if (namedDependencyRegistratorTypes == null)
                return;

            foreach (var namedDependencyRegistratorType in namedDependencyRegistratorTypes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string registratorTypeName;
                if (namedDependencyRegistratorType.ContainingNamespace == null)
                    registratorTypeName = namedDependencyRegistratorType.Name;
                else
                    registratorTypeName = $"{namedDependencyRegistratorType.ContainingNamespace}.{namedDependencyRegistratorType.Name}";

                body.Append($"collection.AddDependency<{registratorTypeName}>();");
            }
        }

        private static void addServicesToBody(CancellationToken cancellationToken, CodeLines body)
        {
            //ToDo: find and register services
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
