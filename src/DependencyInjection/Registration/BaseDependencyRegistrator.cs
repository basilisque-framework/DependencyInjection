﻿/*
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

namespace Basilisque.DependencyInjection.Registration
{
    /// <summary>
    /// Provides a mechanism to register all dependencies and services of a project.
    /// </summary>
    public abstract class BaseDependencyRegistrator : IDependencyRegistrator
    {
        /// <summary>
        /// Registers all dependencies of the given project
        /// </summary>
        /// <param name="collection">The <see cref="DependencyCollection"/> that the dependencies will be registered on.</param>
        public void Initialize(DependencyCollection collection)
        {
            PerformInitialization(collection);
        }

        /// <summary>
        /// Registers all services of the given project
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> that the services will be registered on.</param>
        public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            PerformServiceRegistration(services);
        }

        /// <summary>
        /// Registers all dependencies of the given project
        /// </summary>
        /// <param name="collection">The <see cref="DependencyCollection"/> that the dependencies will be registered on.</param>
        protected abstract void PerformInitialization(DependencyCollection collection);

        /// <summary>
        /// Registers all services of the given project
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> that the services will be registered on.</param>
        protected abstract void PerformServiceRegistration(Microsoft.Extensions.DependencyInjection.IServiceCollection services);
    }
}
