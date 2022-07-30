namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class Generated_IServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void Ensure_Generated_InitializeDependencies_Method_Exists()
        {
            Microsoft.Extensions.DependencyInjection.IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            var builder = services.InitializeDependencies();

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        public void Ensure_Generated_RegisterServices_Method_Exists()
        {
            Microsoft.Extensions.DependencyInjection.IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            services.RegisterServices();
        }
    }
}
