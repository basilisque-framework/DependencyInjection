﻿using Basilisque.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    [Generator]
    public class DependencyInjectionGenerator : IIncrementalGenerator
    {
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
