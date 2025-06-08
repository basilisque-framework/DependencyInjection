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

namespace Basilisque.DependencyInjection.Registration.Annotations;

/// <summary>
/// Attribute for registering the target class/interface at the dependency container
/// </summary>
public class RegisterServiceAttribute : Attribute, IRegisterServiceAttribute
{
    /// <summary>
    /// The scope of the registration
    /// </summary>
    public RegistrationScope Scope { get; protected set; }

    /// <summary>
    /// The type as that the attributed class/interface will be registered
    /// </summary>
    public Type? As { get; set; } = null;

    /// <summary>
    /// Enables or disables the 'ImplementsITypeName'-check
    /// (When enabled, the attributed class gets registered as the implemented interface with the same name with a leading I)
    /// </summary>
    public bool ImplementsITypeName { get; set; } = true;

    /// <summary>
    /// The type of the factory class that will be used to create instances of the service.
    /// </summary>
    /// <remarks>For keyed services this has have a method <see cref="Func{IServiceProvider, Object, TServiceInstance}"/>. For non-keyed services this has to have a method <see cref="Func{IServiceProvider, TServiceInstance}"/></remarks>
    public Type? Factory { get; set; } = null;

    /// <summary>
    /// Optional name of the factory method that will be used to create instances of the service.
    /// </summary>
    /// <remarks>You can specify the method name in case it cannot be automatically resolved.</remarks>
    public string? FactoryMethodName { get; set; } = null;

    /// <summary>
    /// Options service key for keyed registrations.
    /// </summary>
    public object? Key { get; set; } = null;

    /// <summary>
    /// Creates a new <see cref="RegisterServiceAttribute"/>
    /// </summary>
    /// <param name="scope">The scope of the registration</param>
    public RegisterServiceAttribute(RegistrationScope scope)
    {
        Scope = scope;
    }
}
