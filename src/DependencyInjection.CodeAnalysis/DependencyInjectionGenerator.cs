using Basilisque.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    [Generator]
    public class DependencyInjectionGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var rootNamespaceProvider = context.AnalyzerConfigOptionsProvider.Select(DependencyInjectionGeneratorSelectors.RootNamespaceSelector);

            var assemblyNameProvider = context.CompilationProvider.Select(static (cmp, ct) => cmp.AssemblyName);

            var referencedAssemblySymbolsProvider = context.CompilationProvider.Select(DependencyInjectionGeneratorSelectors.ReferencedAssemblySymbolsSelector);

            var combinedValueProviderStub = rootNamespaceProvider.Combine(assemblyNameProvider);

            var combinedValueProviderDependencyImpl = combinedValueProviderStub.Combine(referencedAssemblySymbolsProvider);

            var combinedValueProviderServicesImpl = combinedValueProviderDependencyImpl.Combine(DependencyInjectionGeneratorSelectors.CreateServicesToRegisterValueProvider(context));

            context.RegisterCompilationInfoOutput(combinedValueProviderStub, DependencyInjectionGeneratorOutput.outputStubs);
            context.RegisterImplementationCompilationInfoOutput(combinedValueProviderServicesImpl, DependencyInjectionGeneratorOutput.outputImplementations);
        }
    }
}
