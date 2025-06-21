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

using Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects;
using Basilisque.DependencyInjection.TestAssembly.TestObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class Generated_IImplementsInterfaceDirectlySingleton : BaseRegistrationTests
    {
        [TestMethod]
        public void Ensure_IsService()
        {
            var isService = IsService<IImplementsInterfaceDirectlySingleton>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Check_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsInterfaceDirectlySingleton))).ToList();

            Assert.IsTrue(registeredServices.Count == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlySingleton), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Can_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsInterfaceDirectlySingleton>().ToList();

            Assert.IsTrue(instances.Count == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlySingleton), instances[0].GetType());
        }
    }
}
