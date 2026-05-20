/*
   Copyright 2026 Alexander Stärk

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
using Basilisque.DependencyInjection.Registration.Annotations;

#if BASILISQUE_CODE_ANALYSIS_EXTENSIONSUPPORT
using Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Extensions;
#endif

namespace Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Common;

/// <summary>
/// Provides extension methods for adding service registration code lines to a method body in code generation scenarios.
/// </summary>
/// <remarks>These extension methods assist in generating service registration statements for dependency
/// injection, supporting various registration scopes, keyed registrations, and factory information. Intended for use in
/// source generators or code analysis tools that manipulate or emit C# code for service registration.</remarks>
public static class CodeLinesExtensions
{
    extension(CodeLines methodBody)
    {
#if BASILISQUE_CODE_ANALYSIS
        internal void AddServicesRegistrationToMethodBody(ServiceRegistrationInfo info)
        {
            var servicesToRegister = info.HasRegisteredServices ? info.RegisteredServices : null;

            var registrationScope = info.RegistrationScope.HasValue ? (RegistrationScope?)info.RegistrationScope.Value : null;

            addServicesRegistrationToMethodBody(methodBody, registrationScope, info.ServiceKey, info.FactoryInformation, servicesToRegister, info.ImplementationSymbol!, info.FactoryRegistrationWithoutImplementation);
        }
#endif

#if BASILISQUE_CODE_ANALYSIS_EXTENSIONSUPPORT
        /// <summary>
        /// Adds service registration statements to the method body for the specified services and registration scope.
        /// </summary>
        /// <param name="registrationScope">The scope in which the services should be registered. Determines the lifetime and visibility of the registered services.</param>
        /// <param name="serviceType">The service type symbol to be registered.</param>
        /// <param name="implementationType">The optional implementation type symbol for the service. May be null if there is no implementation deviating from the service type.</param>
        /// <param name="serviceKey">An optional key used to uniquely identify the service registration. May be null if no key is required.</param>
        /// <param name="factoryInformation">Optional information about the factory method or delegate used to create service instances. May be null if not applicable.</param>
        public void AddServicesRegistrationToMethodBody(RegistrationScope registrationScope, INamedTypeSymbol serviceType, INamedTypeSymbol? implementationType = null, string? serviceKey = null, string? factoryInformation = null)
        {
            IEnumerable<INamedTypeSymbol>? servicesToRegister;
            INamedTypeSymbol implementationSymbol;

            if (implementationType is null)
            {
                servicesToRegister = null;
                implementationSymbol = serviceType;
            }
            else
            {
                servicesToRegister = serviceType.ToEnumerable();
                implementationSymbol = implementationType;
            }

            addServicesRegistrationToMethodBody(methodBody, registrationScope, serviceKey, factoryInformation, servicesToRegister, implementationSymbol, factoryRegistrationWithoutImplementation: false);
        }
#endif
    }

    private static void addServicesRegistrationToMethodBody(CodeLines methodBody, RegistrationScope? registrationScope, string? serviceKey, string? factoryInformation, IEnumerable<INamedTypeSymbol>? servicesToRegister, INamedTypeSymbol implementationSymbol, bool factoryRegistrationWithoutImplementation)
    {
        bool isKeyedRegistration = serviceKey is not null;

        string keyedPrefix = "";
        string keyedValue = "";
        if (isKeyedRegistration)
        {
            keyedPrefix = "Keyed";
            keyedValue = serviceKey!;
        }

        if (isKeyedRegistration && factoryInformation is not null)
            keyedValue = $"{keyedValue}, ";

        var registrationScopeName = registrationScope.ToString();

        if (servicesToRegister?.Any() == true)
        {
            INamedTypeSymbol? serviceSymbol;
            INamedTypeSymbol implSymbol;
            foreach (var registeredService in servicesToRegister!)
            {
                if (factoryRegistrationWithoutImplementation)
                {
                    serviceSymbol = null;
                    implSymbol = registeredService;
                }
                else
                {
                    serviceSymbol = registeredService;
                    implSymbol = implementationSymbol;
                }

                var bodyLine = getServiceRegistrationBody(registrationScopeName, isKeyedRegistration, keyedPrefix, keyedValue, factoryInformation, serviceSymbol, implSymbol);
                methodBody.Add(bodyLine);
            }
        }
        else
        {
            var bodyLine = getServiceRegistrationBody(registrationScopeName, isKeyedRegistration, keyedPrefix, keyedValue, factoryInformation, registeredService: null, implementationSymbol);
            methodBody.Add(bodyLine);
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
}
