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

using Basilisque.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection
{
    /// <summary>
    /// Provides extension to the <see cref="IServiceCollection"/> interface
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// This method extends <see cref="IServiceCollection"/> with a mechanism to register dependencies and services for the whole application.
        /// Calling this method creates a <see cref="DependencyRegistratorBuilder{TRootDependencyRegistrator}"/> and initializes the dependency chain.
        /// </summary>
        /// <typeparam name="TRootDependencyRegistrator"></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> all services are registered on.</param>
        /// <returns>A <see cref="DependencyRegistratorBuilder{TRootDependencyRegistrator}"/> that is used to build and execute the chain of <see cref="IDependencyRegistrator"/></returns>
        public static DependencyRegistratorBuilder<TRootDependencyRegistrator> InitializeDependencies<TRootDependencyRegistrator>(this IServiceCollection services)
            where TRootDependencyRegistrator : IDependencyRegistrator, new()
        {
            return new DependencyRegistratorBuilder<TRootDependencyRegistrator>(services);
        }
    }
}
