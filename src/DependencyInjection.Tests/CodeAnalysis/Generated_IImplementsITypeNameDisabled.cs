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
    public class Generated_IImplementsITypeNameDisabled : BaseRegistrationTests
    {
        [TestMethod]
        public void Ensure_BaseInterface_IsService()
        {
            var isService = IsService<IImplementsITypeNameDisabledBase>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Ensure_Interface_IsService()
        {
            var isService = IsService<IImplementsITypeNameDisabled>();

            Assert.IsFalse(isService);
        }

        [TestMethod]
        public void Check_BaseInterface_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeNameDisabledBase))).ToList();

            Assert.IsTrue(registeredServices.Count == 2);

            Assert.AreEqual(typeof(ImplementsITypeNameDisabled), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[0].Lifetime);

            Assert.AreEqual(typeof(ImplementsITypeNameDisabled2), registeredServices[1].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[1].Lifetime);
        }

        [TestMethod]
        public void Check_Interface_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeNameDisabled))).ToList();

            Assert.IsTrue(registeredServices.Count == 0);
        }

        [TestMethod]
        public void Can_BaseInterface_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsITypeNameDisabledBase>().ToList();

            Assert.IsTrue(instances.Count == 2);

            Assert.AreEqual(typeof(ImplementsITypeNameDisabled), instances[0].GetType());
            Assert.AreEqual(typeof(ImplementsITypeNameDisabled2), instances[1].GetType());
        }

        [TestMethod]
        public void Can_Interface_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsITypeNameDisabled>().ToList();

            Assert.IsTrue(instances.Count == 0);
        }
    }
}
