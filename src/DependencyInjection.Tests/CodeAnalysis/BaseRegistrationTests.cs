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

using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    public class BaseRegistrationTests
    {
        private IServiceProviderIsService? _isService;
        private IServiceCollection? _serviceCollection;
        private ServiceProvider? _provider;

        protected IServiceCollection ServiceCollection { get { return _serviceCollection!; } }
        protected ServiceProvider Provider { get { return _provider!; } }

        [TestInitialize]
        public void Initialize()
        {
            _serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            ServiceCollection
                .InitializeDependencies<Basilisque.DependencyInjection.Tests.DependencyRegistrator>()
                .RegisterServices();

            _provider = ServiceCollection.BuildServiceProvider();
            _isService = Provider.GetService<IServiceProviderIsService>()!;
        }

        protected bool IsService<TService>()
        {
            return IsService(typeof(TService));
        }

        protected bool IsService(Type serviceType)
        {
            return _isService!.IsService(serviceType);
        }
    }
}
