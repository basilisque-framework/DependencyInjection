﻿/*
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

using Basilisque.CodeAnalysis.TestSupport.MSTest.SourceGenerators.UnitTests.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.DependencyInjectionGeneratorTests
{
    public abstract class BaseDependencyInjectionGeneratorTest
    {
        [TestMethod]
        public async Task Test()
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
            var refAssemblies = Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net60
                .WithPackages(System.Collections.Immutable.ImmutableArray.Create(new Microsoft.CodeAnalysis.Testing.PackageIdentity("Microsoft.AspNetCore.App.Ref", "6.0.0")));

            //create verifier
            var verifier = new IncrementalSourceGeneratorVerifier<DependencyInjectionGenerator>
            {
                ReferenceAssemblies = refAssemblies,
                //GlobalOptions =
                //{
                //    { "build_property.RootNamespace", "DI.Tests.RNS" }
                //},
                //DiagnosticOptions =
                //{
                //    { "CS1591", Microsoft.CodeAnalysis.ReportDiagnostic.Suppress }
                //},
                //TestState =
                //{
                //    //AnalyzerConfigFiles = { },
                //    Sources = { code }
                //}
            };

            //set the root namespace
            var rns = GetRootNamespace();
            if (!string.IsNullOrWhiteSpace(rns))
                verifier.GlobalOptions.Add("build_property.RootNamespace", rns);

            //set the diagnostic options
            foreach (var diagOp in GetDiagnosticOptions())
                verifier.DiagnosticOptions.Add(diagOp.key, diagOp.value);

            //add the sources
            AddSourcesUnderTest(verifier.TestState.Sources);

            //add a reference to the dependency injection library
            verifier.TestState.AdditionalReferences.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Registration.IDependencyRegistrator).Assembly.Location));

            return verifier;
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

        protected abstract void AddSourcesUnderTest(Microsoft.CodeAnalysis.Testing.SourceFileList sources);

        private List<(string Name, string SourceText)> getExpectedSources()
        {
            var version = System.Reflection.Assembly.GetAssembly(typeof(BaseDependencyInjectionGeneratorTest))?.GetName().Version?.ToString() ?? "1.0.0.0";

            var rootNamespace = GetRootNamespace();

            var result = new List<(string, string)>();

            var assemblyNamespaceImplSrc = getAssemblyNamespaceImplSrc(rootNamespace, version);
            if (assemblyNamespaceImplSrc.HasValue)
                result.Add(assemblyNamespaceImplSrc.Value);

            result.Add(getRootNamespaceStubSrc(rootNamespace, version));
            result.Add(getServiceCollectionExtensionsSrc(rootNamespace, version));
            result.Add(getRootNamespaceImplSrc(rootNamespace, version));

            return result;
        }

        private (string, string)? getAssemblyNamespaceImplSrc(string? rootNamespace, string version)
        {
            if (string.IsNullOrWhiteSpace(rootNamespace))
                return null;

            var src = $@"//------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//   Basilisque.DependencyInjection.CodeAnalysis, {version}
//   
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------

namespace TestProject
{{
    /// <summary>
    /// Registers all dependencies and services of this assembly.
    /// This class mainly exists for performance and simplicity reasons during code compilation.
    /// Although there is technically no reason to not manually interact with this class, you should probably prefer to use the identical class in your root namespace (<see cref=""{rootNamespace}.DependencyRegistrator""/>).
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Basilisque.DependencyInjection.CodeAnalysis"", ""{version}"")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public sealed class DependencyRegistrator : {rootNamespace}.DependencyRegistrator
    {{
    }}
}}";

            return ("DependencyRegistrator_AssemblyNameNamespace.g.cs", src);
        }

        private (string, string) getRootNamespaceStubSrc(string? rootNamespace, string version)
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
//   Basilisque.DependencyInjection.CodeAnalysis, {version}
//   
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------

using Basilisque.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace {rootNamespace}
{{
    /// <summary>
    /// Registers all dependencies and services of this assembly.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Basilisque.DependencyInjection.CodeAnalysis"", ""{version}"")]
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
    }}
}}";

            return (filename, src);
        }

        private (string, string) getServiceCollectionExtensionsSrc(string? rootNamespace, string version)
        {
            if (rootNamespace == null)
                rootNamespace = "TestProject";

            var src = $@"//------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//   Basilisque.DependencyInjection.CodeAnalysis, {version}
//   
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Basilisque.DependencyInjection.Registration;

namespace {rootNamespace}
{{
    /// <summary>
    /// This class contains extension methods for <see cref=""IServiceCollection""/>
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(""Basilisque.DependencyInjection.CodeAnalysis"", ""{version}"")]
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
    }}
}}";

            return ("IServiceCollectionExtensions.g.cs", src);
        }

        private (string, string) getRootNamespaceImplSrc(string? rootNamespace, string version)
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

namespace {rootNamespace}
{{
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
