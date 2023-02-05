using Basilisque.DependencyInjection.Tests.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

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


    [TestClass]
    public class Generated_ExtensionBeingCalled : BaseRegistrationTests
    {
        [TestMethod]
        public void Ensure_IsService()
        {
            var isService = IsService<IMyExtensionInterface1>();

            Assert.IsTrue(isService);
        }

        [TestMethod]
        public void Check_Registered_Types()
        {
            var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IMyExtensionInterface1))).ToList();

            Assert.IsTrue(registeredServices.Count() == 1);

            Assert.AreEqual(typeof(MyExtension1), registeredServices[0].ImplementationType);
            Assert.AreEqual(ServiceLifetime.Transient, registeredServices[0].Lifetime);
        }

        [TestMethod]
        public void Can_Resolve_Instance()
        {
            var instances = Provider.GetServices<IMyExtensionInterface1>().ToList();

            Assert.IsTrue(instances.Count() == 1);

            Assert.AreEqual(typeof(MyExtension1), instances[0].GetType());
        }
    }
}
