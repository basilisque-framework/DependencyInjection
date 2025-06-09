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

using Basilisque.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Threading;

namespace Basilisque.DependencyInjection.CodeAnalysis;

internal static class DependencyInjectionGeneratorOutput
{
    private const string C_DEPENDENCY_REGISTRATOR_ROOTNAMESPACE_COMPILATIONNAME = DependencyInjectionGeneratorSelectors.C_DEPENDENCY_REGISTRATOR_CLASSNAME + "_RootNamespace";
    private const string C_DEPENDENCY_REGISTRATOR_ASSEMBLYNAMENAMESPACE_COMPILATIONNAME = DependencyInjectionGeneratorSelectors.C_DEPENDENCY_REGISTRATOR_CLASSNAME + "_AssemblyNameNamespace";
    private const string C_DEPENDENCY_REGISTRATOR_XMLCOMMENT_DESCRIPTION = @"Registers all dependencies and services of this assembly.";
    private const string C_DEPENDENCYREGISTRATORBUILDER_TYPE = "DependencyRegistratorBuilder";
    private const string C_IDEPENDENCYREGISTRATOR_TYPE = "IDependencyRegistrator";

    internal static void OutputStubs(SourceProductionContext context, ((string? RootNamespace, string? AssemblyName) General, IEnumerable<string>? Extensions) provider, RegistrationOptions registrationOptions)
    {
        if (!checkPreconditions(context, provider.General, registrationOptions))
            return;

        (string mainCompilationName, string mainNamespace, bool hasRootNamespace, string? rootNamespace, string assemblyNameNamespace) = getMainCompilationTarget(provider.General);

        //output the source
        outputDependencyRegistratorStub(registrationOptions, hasRootNamespace, rootNamespace, mainNamespace, mainCompilationName, assemblyNameNamespace, provider.Extensions);
        outputServiceCollectionExtensionMethods(registrationOptions, mainNamespace);
    }

    internal static void OutputImplementations(SourceProductionContext context, (((string? RootNamespace, string? AssemblyName) Options, IEnumerable<INamedTypeSymbol> NamedDependencyRegistratorTypes) GeneralAndDependencies, ImmutableArray<List<ServiceRegistrationInfo>?> ServicesToRegister) provider, RegistrationOptions registrationOptions)
    {
        if (!checkPreconditions(context, provider.GeneralAndDependencies.Options, registrationOptions))
            return;

        (string mainCompilationName, string mainNamespace, _, _, _) = getMainCompilationTarget(provider.GeneralAndDependencies.Options);

        outputDependencyRegistratorImplementation(context, registrationOptions, mainNamespace, mainCompilationName, provider.GeneralAndDependencies.NamedDependencyRegistratorTypes, provider.ServicesToRegister);
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

    private static void outputDependencyRegistratorStub(RegistrationOptions registrationOptions, bool hasRootNamespace, string? rootNamespace, string mainNamespace, string mainCompilationName, string assemblyNameNamespace, IEnumerable<string>? dependencyInjectionExtensions)
    {
        if (hasRootNamespace)
        {
            //output a helper class in the assemblyname as namespace to make it easier to find in completely compiled assemblies
            registrationOptions.CreateCompilationInfo(C_DEPENDENCY_REGISTRATOR_ASSEMBLYNAMENAMESPACE_COMPILATIONNAME, targetNamespace: assemblyNameNamespace)
                .AddNewClassInfo(DependencyInjectionGeneratorSelectors.C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
                {
                    cl.IsSealed = true;
                    cl.BaseClass = string.Concat(rootNamespace, ".", DependencyInjectionGeneratorSelectors.C_DEPENDENCY_REGISTRATOR_CLASSNAME);

                    cl.XmlDocSummary = $@"{C_DEPENDENCY_REGISTRATOR_XMLCOMMENT_DESCRIPTION}
This class mainly exists for performance and simplicity reasons during code compilation.
Although there is technically no reason to not manually interact with this class, you should probably prefer to use the identical class in your root namespace (<see cref=""{cl.BaseClass}""/>).";

                })
                .AddToSourceProductionContext();
        }

        //output the main registrator class
        var compInfo = registrationOptions.CreateCompilationInfo(mainCompilationName, mainNamespace)
            .AddNewClassInfo(DependencyInjectionGeneratorSelectors.C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
            {
                var hasExtensions = dependencyInjectionExtensions?.Any() == true;

                cl.IsPartial = true;
                cl.BaseClass = "BaseDependencyRegistrator";
                cl.XmlDocSummary = C_DEPENDENCY_REGISTRATOR_XMLCOMMENT_DESCRIPTION;

                var performInitializationMethod = new MethodInfo(AccessModifier.Protected, "void", "PerformInitialization")
                {
                    IsOverride = true,
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
                    }
                };

                performInitializationMethod.InheritXmlDoc = true;

                performInitializationMethod.Body.Add(@"
doBeforeInitialization(collection);

initializeDependenciesGenerated(collection);
");

                if (hasExtensions)
                {
                    performInitializationMethod.Body.Add(@"

initializeDependenciesOfExtensions(collection);
");
                }

                performInitializationMethod.Body.Add(@"

doAfterInitialization(collection);
");

                cl.Methods.Add(performInitializationMethod);

                cl.Methods.Add(new MethodInfo(true, "doBeforeInitialization")
                {
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
                    }
                });

                cl.Methods.Add(new MethodInfo(true, "initializeDependenciesGenerated")
                {
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
                    }
                });

                cl.Methods.Add(new MethodInfo(true, "doAfterInitialization")
                {
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
                    }
                });

                if (hasExtensions)
                    outputExtensions(cl, "initialize", "Dependencies", "DependencyCollection", "collection", dependencyInjectionExtensions);


                var performServiceRegistrationMethod = new MethodInfo(AccessModifier.Protected, "void", "PerformServiceRegistration")
                {
                    IsOverride = true,
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "IServiceCollection", "services")
                    }
                };

                performServiceRegistrationMethod.InheritXmlDoc = true;

                performServiceRegistrationMethod.Body.Add(@"
doBeforeRegistration(services);

registerServicesGenerated(services);
");
                if (hasExtensions)
                {
                    performServiceRegistrationMethod.Body.Add(@"

registerServicesOfExtensions(services);
");
                }

                performServiceRegistrationMethod.Body.Add(@"

doAfterRegistration(services);
");
                cl.Methods.Add(performServiceRegistrationMethod);

                cl.Methods.Add(new MethodInfo(true, "doBeforeRegistration")
                {
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "IServiceCollection", "services")
                    }
                });

                cl.Methods.Add(new MethodInfo(true, "registerServicesGenerated")
                {
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "IServiceCollection", "services")
                    }
                });

                cl.Methods.Add(new MethodInfo(true, "doAfterRegistration")
                {
                    Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "IServiceCollection", "services")
                    }
                });

                if (hasExtensions)
                    outputExtensions(cl, "register", "Services", "IServiceCollection", "services", dependencyInjectionExtensions);
            });

        compInfo.Usings.Add("Basilisque.DependencyInjection.Registration");
        compInfo.Usings.Add("Microsoft.Extensions.DependencyInjection");

        compInfo.AddToSourceProductionContext();
    }

    private static void outputServiceCollectionExtensionMethods(RegistrationOptions registrationOptions, string mainNamespace)
    {
        const string C_ISERVICECOLLECTIONEXTENSIONS_CLASSNAME = "IServiceCollectionExtensions";
        const string C_ISERVICECOLLECTION_TYPE = "IServiceCollection";

        var dependencyRegistratorFullQualifiedName = $"{mainNamespace}.{DependencyInjectionGeneratorSelectors.C_DEPENDENCY_REGISTRATOR_CLASSNAME}";
        var dependencyRegistratorBuilderNameWithGeneric = $"{C_DEPENDENCYREGISTRATORBUILDER_TYPE}<{dependencyRegistratorFullQualifiedName}>";
        var dependencyRegistratorBuilderNameWithGenericForXmlDoc = $"{C_DEPENDENCYREGISTRATORBUILDER_TYPE}{{TDependencyRegistrator}}";

        var ci = registrationOptions.CreateCompilationInfo(C_ISERVICECOLLECTIONEXTENSIONS_CLASSNAME, mainNamespace);
        ci.Usings.Add("Microsoft.Extensions.DependencyInjection");
        ci.Usings.Add("Basilisque.DependencyInjection.Registration");
        ci.AddNewClassInfo(C_ISERVICECOLLECTIONEXTENSIONS_CLASSNAME, cl =>
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
            initializeDependenciesExtensionMethod.Body.Add($@"return Basilisque.DependencyInjection.IServiceCollectionExtensions.InitializeDependencies<{dependencyRegistratorFullQualifiedName}>(services);");
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
            registerServicesExtensionMethod.Body.Add($@"Basilisque.DependencyInjection.IServiceCollectionExtensions.InitializeDependencies<{dependencyRegistratorFullQualifiedName}>(services).RegisterServices();");
            cl.Methods.Add(registerServicesExtensionMethod);
        })
        .AddToSourceProductionContext();
    }

    private static void outputDependencyRegistratorImplementation(SourceProductionContext context, RegistrationOptions registrationOptions, string mainNamespace, string mainCompilationName, IEnumerable<INamedTypeSymbol> namedDependencyRegistratorTypes, ImmutableArray<List<ServiceRegistrationInfo>?> servicesToRegister)
    {
        //output implementation of the main registrator class
        var ci = registrationOptions.CreateCompilationInfo($"{mainCompilationName}.impl", mainNamespace);
        ci.AddGeneratedCodeAttributes = false;
        ci.Usings.Add("Basilisque.DependencyInjection.Registration");
        ci.Usings.Add("Microsoft.Extensions.DependencyInjection");
        ci.AddNewClassInfo(DependencyInjectionGeneratorSelectors.C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
        {
            cl.IsPartial = true;

            var initializeDependenciesGeneratedMethod = new MethodInfo(true, "initializeDependenciesGenerated")
            {
                Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
                    }
            };
            initializeDependenciesGeneratedMethod.Body.Add(@"/* initialize dependencies - generated from assembly dependencies */");
            addDependenciesToBody(context.CancellationToken, initializeDependenciesGeneratedMethod.Body, namedDependencyRegistratorTypes);
            cl.Methods.Add(initializeDependenciesGeneratedMethod);

            var registerServicesGeneratedMethod = new MethodInfo(true, "registerServicesGenerated")
            {
                Parameters = {
                        new ParameterInfo(ParameterKind.Ordinary, "Microsoft.Extensions.DependencyInjection.IServiceCollection", "services")
                    }
            };
            registerServicesGeneratedMethod.Body.Add(@"/* register services - generated from the current project */");
            addServicesToBody(context, registerServicesGeneratedMethod.Body, servicesToRegister);
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

            body.Add($"collection.AddDependency<{registratorTypeName}>();");
        }
    }

    private static void addServicesToBody(SourceProductionContext context, CodeLines body, ImmutableArray<List<ServiceRegistrationInfo>?> serviceInfosListToRegister)
    {
        foreach (var serviceInfosToRegister in serviceInfosListToRegister)
        {
            if (serviceInfosToRegister == null)
                continue;

            foreach (var item in serviceInfosToRegister)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!validateRegistrationInfo(item))
                    continue;

                bool isKeyedRegistration = item.ServiceKey is not null;

                string keyedPrefix = "";
                string keyedValue = "";
                if (isKeyedRegistration)
                {
                    keyedPrefix = "Keyed";
                    keyedValue = item.ServiceKey!;
                }

                if (!tryGetFactoryInformation(context, item, isKeyedRegistration, out var factoryInformation))
                    continue;

                if (isKeyedRegistration && factoryInformation is not null)
                    keyedValue = $"{keyedValue}, ";

                var registrationScope = item.RegistrationScope.ToString();

                if (item.HasRegisteredServices)
                {
                    foreach (var registeredService in item.RegisteredServices!)
                    {
                        var bodyLine = getServiceRegistrationBody(registrationScope, isKeyedRegistration, keyedPrefix, keyedValue, factoryInformation, registeredService, item.ImplementationSymbol!);
                        body.Add(bodyLine);
                    }
                }
                else
                {
                    var bodyLine = getServiceRegistrationBody(registrationScope, isKeyedRegistration, keyedPrefix, keyedValue, factoryInformation, registeredService: null, item.ImplementationSymbol!);
                    body.Add(bodyLine);
                }
            }
        }
    }

    private static string getServiceRegistrationBody(string registrationScope, bool isKeyedRegistration, string keyedPrefix, string keyedValue, string? factoryInformation, INamedTypeSymbol? registeredService, INamedTypeSymbol implementationSymbol)
    {
        var symbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);

        string? registeredServiceInfo = null;
        bool registeredServiceIsOpenGeneric;
        if (registeredService is not null)
        {
            registeredServiceIsOpenGeneric = registeredService.IsUnboundGenericType;

            string registeredServiceDisplayString;
            if (registeredServiceIsOpenGeneric)
                registeredServiceDisplayString = registeredService.ConstructUnboundGenericType().ToDisplayString(symbolDisplayFormat);
            else
                registeredServiceDisplayString = registeredService.ToDisplayString(symbolDisplayFormat);

            if (registeredServiceIsOpenGeneric || implementationSymbol.IsGenericType)
                registeredServiceInfo = $"typeof({registeredServiceDisplayString}), ";
            else
                registeredServiceInfo = $"{registeredServiceDisplayString}, ";
        }
        else
            registeredServiceIsOpenGeneric = false;

        string implementationInfo;
        if (implementationSymbol.IsGenericType)
            implementationInfo = implementationSymbol.ConstructUnboundGenericType().ToDisplayString(symbolDisplayFormat);
        else
            implementationInfo = implementationSymbol.ToDisplayString(symbolDisplayFormat);

        string result;
        if (registeredServiceIsOpenGeneric || implementationSymbol.IsGenericType)
        {
            if (isKeyedRegistration || factoryInformation is not null)
                keyedValue = $", {keyedValue}";

            result = $"services.Add{keyedPrefix}{registrationScope}({registeredServiceInfo}typeof({implementationInfo}){keyedValue}{factoryInformation});";
        }
        else
        {
            result = $"services.Add{keyedPrefix}{registrationScope}<{registeredServiceInfo}{implementationInfo}>({keyedValue}{factoryInformation});";
        }

        return result;
    }

    private static bool tryGetFactoryInformation(SourceProductionContext context, ServiceRegistrationInfo item, bool isKeyedRegistration, out string? factoryInformation)
    {
        factoryInformation = null;

        if (item.FactoryType is null)
        {
            if (item.FactoryMethodName is not null)
            {
                // The factory method name is defined, but no factory type is defined.
                // This doesn't make sense, so we report a diagnostic.

                var location = item.ImplementationSyntaxNode?.GetLocation() ?? Location.None;
                var factoryTypeDiagnostic = Diagnostic.Create(DiagnosticDescriptors.FactoryTypeNotDefined, location, item.FactoryMethodName);
                context.ReportDiagnostic(factoryTypeDiagnostic);
                return false;
            }

            // No factory type or factory method name defined.
            // This is fine, so we just return true and register the service without a factory.
            return true;
        }

        var factoryTypeName = item.FactoryType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var factoryMethodName = getValidFactoryMethod(item.FactoryType, isKeyedRegistration, item.FactoryMethodName);
        if (factoryMethodName is null)
        {
            // No valid factory method found, so we report a diagnostic.
            var location = item.ImplementationSyntaxNode?.GetLocation() ?? Location.None;
            Diagnostic factoryMethodDiagnostic;
            if (item.FactoryMethodName is null)
                factoryMethodDiagnostic = Diagnostic.Create(DiagnosticDescriptors.FactoryMethodNotFound, location, factoryTypeName);
            else
                factoryMethodDiagnostic = Diagnostic.Create(DiagnosticDescriptors.FactoryMethodNameIsInvalid, location, item.FactoryMethodName, factoryTypeName);
            context.ReportDiagnostic(factoryMethodDiagnostic);
            return false;
        }

        factoryInformation = $"{factoryTypeName}.{factoryMethodName}";
        return true;
    }

    private static bool validateRegistrationInfo(ServiceRegistrationInfo? registrationInfo)
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

    private static void outputExtensions(ClassInfo cl, string prefix, string type, string paramType, string paramName, IEnumerable<string>? dependencyInjectionExtensions)
    {
        var extensionsMethod = new MethodInfo(false, $"{prefix}{type}OfExtensions")
        {
            Parameters = {
                new ParameterInfo(ParameterKind.Ordinary, paramType, paramName)
            }
        };

        cl.Methods.Add(extensionsMethod);

        string? extNewLine = null;
        foreach (var extension in dependencyInjectionExtensions!)
        {
            extensionsMethod.Body.Add($@"{extNewLine}
{prefix}Extension_{extension}({paramName});
");


            /*
             var syntaxTree = ... // syntaxTree you want to know the newline settings for
var options = context.AnalyzerConfigOptions.GetOptions(syntaxTree);
if (options.TryGetValue("end_of_line", out var newline))
{
// do something with newline
} 
             */
            /*
             var workspace = ... // get a reference to a workspace
var newline = workspace.GetOption(new OptionKey(FormattingOptions.NewLine, LanguageNames.CSharp));
             */
            if (extNewLine is null)
                extNewLine = SyntaxFactory.ElasticCarriageReturnLineFeed.ToString();

            cl.Methods.Add(new MethodInfo(true, $"{prefix}Extension_{extension}")
            {
                Parameters = {
                    new ParameterInfo(ParameterKind.Ordinary, paramType, paramName)
                }
            });
        }
    }

    private static string? getValidFactoryMethod(INamedTypeSymbol factoryType, bool isKeyed, string? expectedMethodName)
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

            return factoryMethod?.Name;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
