using Basilisque.DependencyInjection.Tests.TestObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class Generated_IImplementsInterfaceDirectlyScoped : BaseRegistrationTests
    {
        [TestMethod]
        public void Ensure_IsService()
        {
            var isService = IsService<IImplementsInterfaceDirectlyScoped>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Check_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsInterfaceDirectlyScoped))).ToList();

            Assert.IsTrue(registeredServices.Count() == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlyScoped), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Scoped, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Can_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsInterfaceDirectlyScoped>().ToList();

            Assert.IsTrue(instances.Count() == 1);

            Assert.AreEqual(typeof(ImplementsInterfaceDirectlyScoped), instances[0].GetType());
        }
    }
}
