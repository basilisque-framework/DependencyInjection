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

namespace Basilisque.DependencyInjection.Registration.Annotations
{
    /// <summary>
    /// Specifies the lifetime of a service in an Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
#if BASILISQUE_CODE_ANALYSIS
    internal
#else
    public
#endif
        enum RegistrationScope
    {
        /// <summary>
        /// Specifies that a new instance of the service will be created every time it is requested.
        /// </summary>
        Transient,

        /// <summary>
        /// Specifies that a new instance of the service will be created for each scope.
        /// </summary>
        /// <remarks>In ASP.NET Core applications a scope is created around each server request.</remarks>
        Scoped,

        /// <summary>
        /// Specifies that a single instance of the service will be created.
        /// </summary>
        Singleton
    }
}
