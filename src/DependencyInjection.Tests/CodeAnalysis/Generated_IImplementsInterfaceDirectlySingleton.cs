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

            Assert.IsTrue(registeredServices.Count() == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlySingleton), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Can_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsInterfaceDirectlySingleton>().ToList();

            Assert.IsTrue(instances.Count() == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlySingleton), instances[0].GetType());
        }
    }
}
