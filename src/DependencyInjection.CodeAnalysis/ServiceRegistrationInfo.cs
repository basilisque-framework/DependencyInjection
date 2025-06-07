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

namespace Basilisque.DependencyInjection.CodeAnalysis;

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
    public INamedTypeSymbol? FactoryType { get; set; } = null;
    public string? FactoryMethodName { get; set; } = null;

    public ServiceRegistrationInfo()
    { }
}
