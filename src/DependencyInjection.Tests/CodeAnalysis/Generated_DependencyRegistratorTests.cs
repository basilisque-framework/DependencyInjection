namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class Generated_DependencyRegistratorTests
    {
        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_Exists()
        {
            var registrator = new Basilisque.DependencyInjection.Tests.DependencyRegistrator();

            Assert.IsNotNull(registrator);
        }

        [TestMethod]
        public void Ensure_CanCall_GenericInitializeDependencies_WithGenerated_DependencyRegistrator()
        {
            Microsoft.Extensions.DependencyInjection.IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            var builder = services.InitializeDependencies<Basilisque.DependencyInjection.Tests.DependencyRegistrator>();

            Assert.IsNotNull(builder);
        }
    }
}
