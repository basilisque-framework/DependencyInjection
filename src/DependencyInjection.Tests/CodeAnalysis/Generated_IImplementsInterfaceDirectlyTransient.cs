using Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects;
using Basilisque.DependencyInjection.TestAssembly.TestObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class Generated_IImplementsInterfaceDirectlyTransient : BaseRegistrationTests
    {
        [TestMethod]
        public void Ensure_IsService()
        {
            var isService = IsService<IImplementsInterfaceDirectlyTransient>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Check_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsInterfaceDirectlyTransient))).ToList();

            Assert.IsTrue(registeredServices.Count() == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlyTransient), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Transient, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Can_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsInterfaceDirectlyTransient>().ToList();

            Assert.IsTrue(instances.Count() == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlyTransient), instances[0].GetType());
        }
    }
}
