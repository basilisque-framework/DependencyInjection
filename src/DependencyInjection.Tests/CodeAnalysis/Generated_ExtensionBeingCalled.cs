/*
   Copyright 2023-2026 Alexander Stärk

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

using Basilisque.DependencyInjection.Tests.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Basilisque.DependencyInjection.Tests
{
    public partial class DependencyRegistrator
    {
        partial void registerExtension_MyTestExtension1(IServiceCollection services)
        {
            services.AddTransient<IMyExtensionInterface1, MyExtension1>();
        }
    }
}

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    internal interface IMyExtensionInterface1
    { }

    internal class MyExtension1 : IMyExtensionInterface1
    { }


    public class Generated_ExtensionBeingCalled : BaseRegistrationTests
    {
        [Test]

        public void Ensure_IsService()
        {
            var isService = IsService<IMyExtensionInterface1>();

            isService.ShouldBeTrue();
        }

        [Test]
        public void Check_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IMyExtensionInterface1))).ToList();

            registeredServices.Count.ShouldBe(1);

            registeredServices[0].ImplementationType.ShouldBe(typeof(MyExtension1));
            registeredServices[0].Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Test]
        public void Can_Resolve_Instance()
        {
            var instances = Provider.GetServices<IMyExtensionInterface1>().ToList();

            instances.Count.ShouldBe(1);

            instances[0].GetType().ShouldBe(typeof(MyExtension1));
        }
    }
}
