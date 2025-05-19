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

using Basilisque.CodeAnalysis.TestSupport.SourceGenerators.UnitTests.Verifiers;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.DependencyInjectionGeneratorTests
{
    public abstract class BaseDependencyInjectionGeneratorTest : BaseDependencyInjectionGeneratorTest<IncrementalSourceGeneratorVerifier<DependencyInjectionGenerator>>
    { }

    public abstract class BaseDependencyInjectionGeneratorTest<TGenerator>
        where TGenerator : IncrementalSourceGeneratorVerifier<DependencyInjectionGenerator>, new()
    {
        [TestMethod]
        public virtual async Task Test()
        {
            var verifier = GetVerifier();

            var expectedSources = getExpectedSources();
            foreach (var expectedSource in expectedSources)
            {
                verifier.TestState.GeneratedSources.Add((typeof(DependencyInjectionGenerator), expectedSource.Name, SourceText.From(expectedSource.SourceText, Encoding.UTF8)));
            }

            await verifier.RunAsync();
        }

        protected virtual IncrementalSourceGeneratorVerifier<DependencyInjectionGenerator> GetVerifier()
        {
            //define reference assemblies
#if NET462
            //the tests compiled for NET462 actually test the NETSTANDARD2_0 assemblies
            var refAssemblies = new Microsoft.CodeAnalysis.Testing.ReferenceAssemblies(
                "netstandard2.0",
                new Microsoft.CodeAnalysis.Testing.PackageIdentity(
                    "NETStandard.Library",
                    "2.0.3"),
                @"build\netstandard2.0\ref")
                .AddAssemblies(ImmutableArray.Create("netstandard"))
                .WithPackages(System.Collections.Immutable.ImmutableArray.Create(new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.Extensions.DependencyInjection", "6.0.0")));
#elif NETSTANDARD2_1
            var refAssemblies = new Microsoft.CodeAnalysis.Testing.ReferenceAssemblies(
                "netstandard2.1",
                new Microsoft.CodeAnalysis.Testing.PackageIdentity(
                    "NETStandard.Library",
                    "2.0.3"),
                @"build\netstandard2.1\ref")
                .AddAssemblies(ImmutableArray.Create("netstandard"));
#elif NET6_0
            var refAssemblies = Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net60
                .WithPackages(System.Collections.Immutable.ImmutableArray.Create(new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.AspNetCore.App.Ref", "6.0.0")));
#elif NET7_0
            var refAssemblies = new ReferenceAssemblies(
                        "net7.0",
                        new PackageIdentity(
                            "Microsoft.NETCore.App.Ref",
                            "7.0.0"),
                        Path.Combine("ref", "net7.0"))
                .WithPackages(ImmutableArray.Create(new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.AspNetCore.App.Ref", "7.0.0")));
#elif NET8_0
            var refAssemblies = new ReferenceAssemblies(
                        "net8.0",
                        new PackageIdentity(
                            "Microsoft.NETCore.App.Ref",
                            "8.0.0"),
                        Path.Combine("ref", "net8.0"))
                .WithPackages([new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.AspNetCore.App.Ref", "8.0.0")]);
#elif NET9_0
            var refAssemblies = new ReferenceAssemblies(
                        "net9.0",
                        new PackageIdentity(
                            "Microsoft.NETCore.App.Ref",
                            "9.0.0"),
                        Path.Combine("ref", "net9.0"))
                .WithPackages([new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.AspNetCore.App.Ref", "9.0.0")]);
#else
            throw new PlatformNotSupportedException("Please define reference assemblies for your platform!");
#endif

            //create verifier
            var verifier = new TGenerator
            {
                ReferenceAssemblies = refAssemblies
                //TestState =
                //{
                //    //AnalyzerConfigFiles = { }
                //}
            };

            //set the root namespace
            var rns = GetRootNamespace();
            if (!string.IsNullOrWhiteSpace(rns))
                verifier.GlobalOptions.Add("build_property.RootNamespace", rns!);

            //set the diagnostic options
            foreach (var diagOp in GetDiagnosticOptions())
                verifier.DiagnosticOptions.Add(diagOp.key, diagOp.value);

            foreach (var expDiag in GetExpectedDiagnostics())
                verifier.ExpectedDiagnostics.Add(expDiag);

            //add the sources
            AddSourcesUnderTest(verifier.TestState.Sources);

            // add empty source file as a workaround for the base class checking if any source file exists
            if (!verifier.TestState.Sources.Any())
                verifier.TestState.Sources.Add("");

            //add a reference to the dependency injection library
            verifier.TestState.AdditionalReferences.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Registration.IDependencyRegistrator).Assembly.Location));

            return (IncrementalSourceGeneratorVerifier<DependencyInjectionGenerator>)verifier;
        }

        protected virtual string? GetRootNamespace()
        {
            return "DI.Tests.RNS";
        }

        protected virtual IEnumerable<(string key, Microsoft.CodeAnalysis.ReportDiagnostic value)> GetDiagnosticOptions()
        {
            //we can return diagnostic options like this:
            //yield return ("CS1591", Microsoft.CodeAnalysis.ReportDiagnostic.Suppress);
            //yield return ("CS159?", Microsoft.CodeAnalysis.ReportDiagnostic.???);

            yield break;
        }

        protected virtual IEnumerable<Microsoft.CodeAnalysis.Testing.DiagnosticResult> GetExpectedDiagnostics()
        {
            //we can return expected diagnostics like this:
            //yield return new Microsoft.CodeAnalysis.Testing.DiagnosticResult("CS1591", Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

            yield break;
        }

        protected abstract void AddSourcesUnderTest(Microsoft.CodeAnalysis.Testing.SourceFileList sources);

        private List<(string Name, string SourceText)> getExpectedSources()
        {
            var assemblyNameInfo = System.Reflection.Assembly.GetAssembly(typeof(DependencyInjectionGenerator))?.GetName();

            string assemblyName, version;
            if (assemblyNameInfo is null)
            {
                version = "1.0.0.0";
                assemblyName = "Basilisque.DependencyInjection.CodeAnalysis-1.0-Alpha";
            }
            else
            {
                version = assemblyNameInfo.Version?.ToString() ?? "1.0.0.0";
                assemblyName = assemblyNameInfo.Name ?? "Basilisque.DependencyInjection.CodeAnalysis-1.0-Alpha";
            }

            var rootNamespace = GetRootNamespace();

            var result = new List<(string, string)>();

            var assemblyNamespaceImplSrc = getAssemblyNamespaceImplSrc(rootNamespace, assemblyName, version);
            if (assemblyNamespaceImplSrc.HasValue)
                result.Add(assemblyNamespaceImplSrc.Value);

            result.Add(getRootNamespaceStubSrc(rootNamespace, assemblyName, version));
            result.Add(getServiceCollectionExtensionsSrc(rootNamespace, assemblyName, version));
            result.Add(getRootNamespaceImplSrc(rootNamespace, assemblyName, version));

            return result;
        }

        private (string, string)? getAssemblyNamespaceImplSrc(string? rootNamespace, string assemblyName, string version)
        {
            if (string.IsNullOrWhiteSpace(rootNamespace))
                return null;

            var src = $@"//------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//   {assemblyName}, {version}
//   
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------

namespace TestProject;

/// <summary>
/// Registers all dependencies and services of this assembly.
/// This class mainly exists for performance and simplicity reasons during code compilation.
/// Although there is technically no reason to not manually interact with this class, you should probably prefer to use the identical class in your root namespace (<see cref=""{rootNamespace}.DependencyRegistrator""/>).
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{assemblyName}"", ""{version}"")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
public sealed class DependencyRegistrator : {rootNamespace}.DependencyRegistrator
{{
}}";

            return ("DependencyRegistrator_AssemblyNameNamespace.g.cs", src);
        }

        private (string, string) getRootNamespaceStubSrc(string? rootNamespace, string assemblyName, string version)
        {
            string filename;
            if (rootNamespace == null)
            {
                rootNamespace = "TestProject";
                filename = "DependencyRegistrator_AssemblyNameNamespace.g.cs";
            }
            else
                filename = "DependencyRegistrator_RootNamespace.g.cs";

            var src = $@"//------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//   {assemblyName}, {version}
//   
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------

using Basilisque.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace {rootNamespace};

/// <summary>
/// Registers all dependencies and services of this assembly.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{assemblyName}"", ""{version}"")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
public partial class DependencyRegistrator : BaseDependencyRegistrator
{{
    /// <inheritdoc />
    protected override void PerformInitialization(DependencyCollection collection)
    {{
        doBeforeInitialization(collection);
        
        initializeDependenciesGenerated(collection);
        
        doAfterInitialization(collection);
    }}
    
    partial void doBeforeInitialization(DependencyCollection collection);
    
    partial void initializeDependenciesGenerated(DependencyCollection collection);
    
    partial void doAfterInitialization(DependencyCollection collection);
    
    /// <inheritdoc />
    protected override void PerformServiceRegistration(IServiceCollection services)
    {{
        doBeforeRegistration(services);
        
        registerServicesGenerated(services);
        
        doAfterRegistration(services);
    }}
    
    partial void doBeforeRegistration(IServiceCollection services);
    
    partial void registerServicesGenerated(IServiceCollection services);
    
    partial void doAfterRegistration(IServiceCollection services);
}}";

            return (filename, src);
        }

        private (string, string) getServiceCollectionExtensionsSrc(string? rootNamespace, string assemblyName, string version)
        {
            if (rootNamespace == null)
                rootNamespace = "TestProject";

            var src = $@"//------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//   {assemblyName}, {version}
//   
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Basilisque.DependencyInjection.Registration;

namespace {rootNamespace};

/// <summary>
/// This class contains extension methods for <see cref=""IServiceCollection""/>
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{assemblyName}"", ""{version}"")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
public static class IServiceCollectionExtensions
{{
    /// <summary>
    /// This method extends <see cref=""IServiceCollection""/> with a mechanism to register dependencies and services for the whole application.
    /// Calling this method creates a <see cref=""DependencyRegistratorBuilder{{TDependencyRegistrator}}""/> and initializes the dependency chain.
    /// </summary>
    /// <param name=""services"">The <see cref=""IServiceCollection""/> all services are registered on.</param>
    /// <returns>A <see cref=""DependencyRegistratorBuilder{{TDependencyRegistrator}}""/> that is used to build and execute the chain of <see cref=""IDependencyRegistrator""/></returns>
    public static DependencyRegistratorBuilder<{rootNamespace}.DependencyRegistrator> InitializeDependencies(this IServiceCollection services)
    {{
        return Basilisque.DependencyInjection.IServiceCollectionExtensions.InitializeDependencies<{rootNamespace}.DependencyRegistrator>(services);
    }}
    
    /// <summary>
    /// This method extends <see cref=""IServiceCollection""/> with a mechanism to register dependencies and services for the whole application.
    /// Calling this method creates a <see cref=""DependencyRegistratorBuilder{{TDependencyRegistrator}}""/>, initializes the dependency chain and executes the registration of all services.
    /// For more control over the details of this process use <see cref=""InitializeDependencies""/> instead.
    /// </summary>
    /// <param name=""services"">The <see cref=""IServiceCollection""/> all services are registered on.</param>
    public static void RegisterServices(this IServiceCollection services)
    {{
        Basilisque.DependencyInjection.IServiceCollectionExtensions.InitializeDependencies<{rootNamespace}.DependencyRegistrator>(services).RegisterServices();
    }}
}}";

            return ("IServiceCollectionExtensions.g.cs", src);
        }

        private (string, string) getRootNamespaceImplSrc(string? rootNamespace, string assemblyName, string version)
        {
            string filename;
            if (rootNamespace == null)
            {
                rootNamespace = "TestProject";
                filename = "DependencyRegistrator_AssemblyNameNamespace.impl.g.cs";
            }
            else
                filename = "DependencyRegistrator_RootNamespace.impl.g.cs";

            var registeredServicesSource = GetRegisteredServicesSource() ?? "";

            var src = $@"using Basilisque.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace {rootNamespace};

public partial class DependencyRegistrator
{{
    partial void initializeDependenciesGenerated(DependencyCollection collection)
    {{
        /* initialize dependencies - generated from assembly dependencies */
    }}
    
    partial void registerServicesGenerated(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {{
        /* register services - generated from the current project */{registeredServicesSource}
    }}
}}";

            return (filename, src);
        }

        protected virtual string? GetRegisteredServicesSource()
        {
            return null;
        }
    }
}
