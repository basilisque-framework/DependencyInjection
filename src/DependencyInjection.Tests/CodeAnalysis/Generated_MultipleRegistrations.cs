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
    public class Generated_MultipleRegistrations : BaseRegistrationTests
    {
        [TestMethod]
        public void Ensure_MultipleRegistrations_IsService()
        {
            var isService = IsService<MultipleRegistrations>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Ensure_IMultipleRegistration_IsService()
        {
            var isService = IsService<IMultipleRegistration>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Ensure_IMultipleRegistrations_IsService()
        {
            var isService = IsService<IMultipleRegistrations>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Check_MultipleRegistrations_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(MultipleRegistrations))).ToList();

            Assert.IsTrue(registeredServices.Count == 1);

            Assert.AreEqual(typeof(MultipleRegistrations), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Scoped, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Check_IMultipleRegistration_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IMultipleRegistration))).ToList();

            Assert.IsTrue(registeredServices.Count == 1);

            Assert.AreEqual(typeof(MultipleRegistrations), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Check_IMultipleRegistrations_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IMultipleRegistrations))).ToList();

            Assert.IsTrue(registeredServices.Count == 1);

            Assert.AreEqual(typeof(MultipleRegistrations), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Transient, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Can_MultipleRegistrations_Resolve_Instance()
        {
            var instances = Provider.GetServices<MultipleRegistrations>().ToList();

            Assert.IsTrue(instances.Count == 1);

            Assert.AreEqual(typeof(MultipleRegistrations), instances[0].GetType());
        }

        [TestMethod]
        public void Can_IMultipleRegistration_Resolve_Instance()
        {
            var instances = Provider.GetServices<IMultipleRegistration>().ToList();

            Assert.IsTrue(instances.Count == 1);

            Assert.AreEqual(typeof(MultipleRegistrations), instances[0].GetType());
        }

        [TestMethod]
        public void Can_IMultipleRegistrations_Resolve_Instance()
        {
            var instances = Provider.GetServices<IMultipleRegistrations>().ToList();

            Assert.IsTrue(instances.Count == 1);

            Assert.AreEqual(typeof(MultipleRegistrations), instances[0].GetType());
        }
    }
}
