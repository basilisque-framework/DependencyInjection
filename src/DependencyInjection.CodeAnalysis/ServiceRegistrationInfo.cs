using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Basilisque.DependencyInjection.CodeAnalysis
{
    internal class ServiceRegistrationInfo
    {
        private IList<INamedTypeSymbol>? _registeredServices = null;

        public Registration.Annotations.RegistrationScope? RegistrationScope { get; set; } = null;
        public Microsoft.CodeAnalysis.SyntaxNode? ImplementationSyntaxNode { get; set; } = null;
        public INamedTypeSymbol? ImplementationSymbol { get; set; } = null;
        public IList<INamedTypeSymbol> RegisteredServices
        {
            get
            {
                if (_registeredServices == null)
                    _registeredServices = new List<INamedTypeSymbol>();

                return _registeredServices;
            }
        }
        public bool HasRegisteredServices { get { return _registeredServices?.Count > 0; } }
        public ServiceRegistrationInfo()
        { }
    }
}
