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

namespace Basilisque.DependencyInjection.Registration.Annotations
{
    /// <summary>
    /// Attribute for registering the target class/interface at the dependency container with <see cref="RegistrationScope.Singleton"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    [RegisterService(RegistrationScope.Singleton)]
    public class RegisterServiceSingletonAttribute : RegisterServiceAttribute
    {
        /// <summary>
        /// Creates a new <see cref="RegisterServiceSingletonAttribute"/>
        /// </summary>
        public RegisterServiceSingletonAttribute()
            : base(RegistrationScope.Singleton)
        { }
    }
}
