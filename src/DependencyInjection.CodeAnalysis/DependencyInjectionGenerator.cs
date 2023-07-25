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

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    /// <summary>
    /// A source generator that generates code to register classes at the dependency container
    /// </summary>
    [Generator]
    public class DependencyInjectionGenerator : IIncrementalGenerator
    {
        ///<inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //get providers
            var rootNamespaceProvider = context.AnalyzerConfigOptionsProvider.Select(DependencyInjectionGeneratorSelectors.RootNamespaceSelector);

            var extensionsProvider = context.AnalyzerConfigOptionsProvider.Select(DependencyInjectionGeneratorSelectors.DependencyInjectionExtensionsSelector);

            var assemblyNameProvider = context.CompilationProvider.Select(static (cmp, ct) => cmp.AssemblyName);

            var referencedAssemblySymbolsProvider = context.CompilationProvider.Select(DependencyInjectionGeneratorSelectors.ReferencedAssemblySymbolsSelector);

            //combine providers for stub output
            var combinedValueProviderStub = rootNamespaceProvider.Combine(assemblyNameProvider);
            var combinedValueProviderExtensions = combinedValueProviderStub.Combine(extensionsProvider);

            //combine stub output providers with other providers for implementation output
            var combinedValueProviderDependencyImpl = combinedValueProviderStub.Combine(referencedAssemblySymbolsProvider);
            var combinedValueProviderServicesImpl = combinedValueProviderDependencyImpl.Combine(DependencyInjectionGeneratorSelectors.CreateServicesToRegisterValueProvider(context));


            //write output
            context.RegisterCompilationInfoOutput(combinedValueProviderExtensions, DependencyInjectionGeneratorOutput.OutputStubs);
            context.RegisterImplementationCompilationInfoOutput(combinedValueProviderServicesImpl, DependencyInjectionGeneratorOutput.OutputImplementations);
        }
    }
}
