using Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects;
using Basilisque.DependencyInjection.TestAssembly.TestObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class Generated_IImplementsITypeName : BaseRegistrationTests
    {
        [TestMethod]
        public void Ensure_BaseInterface_IsService()
        {
            var isService = IsService<IImplementsITypeNameBase>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Ensure_Interface_IsService()
        {
            var isService = IsService<IImplementsITypeName>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Check_BaseInterface_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeNameBase))).ToList();

            Assert.IsTrue(registeredServices.Count() == 2);

            Assert.AreEqual(typeof(ImplementsITypeName), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[0].Lifetime);

            Assert.AreEqual(typeof(ImplementsITypeName2), registeredServices[1].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[1].Lifetime);
        }

        [TestMethod]
        public void Check_Interface_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeName))).ToList();

            Assert.IsTrue(registeredServices.Count() == 1);

            Assert.AreEqual(typeof(ImplementsITypeName), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Can_BaseInterface_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsITypeNameBase>().ToList();

            Assert.IsTrue(instances.Count() == 2);

            Assert.AreEqual(typeof(ImplementsITypeName), instances[0].GetType());
            Assert.AreEqual(typeof(ImplementsITypeName2), instances[1].GetType());
        }

        [TestMethod]
        public void Can_Interface_Resolve_Instance()
        {
            var instances = Provider.GetServices<IImplementsITypeName>().ToList();

            Assert.IsTrue(instances.Count() == 1);

            Assert.AreEqual(typeof(ImplementsITypeName), instances[0].GetType());
        }
    }
}
