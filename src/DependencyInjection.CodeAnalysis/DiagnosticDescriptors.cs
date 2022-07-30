using Microsoft.CodeAnalysis;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    internal static class DiagnosticDescriptors
    {
        public static DiagnosticDescriptor MissingAssemblyName { get { return new DiagnosticDescriptor("BAS-DI-001", "The assembly name could not be determined.", $"The name of the assembly is empty", "Basilisque.DependencyInjection", DiagnosticSeverity.Error, true); } }
    }
}
