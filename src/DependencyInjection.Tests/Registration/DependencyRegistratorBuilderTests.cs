namespace Basilisque.DependencyInjection.Tests.Registration
{
    [TestClass]
    public class DependencyRegistratorBuilderTests
    {
        [TestMethod]
        public void When_Services_Is_Null_Throw_Exception()
        {
            Microsoft.Extensions.DependencyInjection.IServiceCollection services = null!;

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                DependencyInjection.Registration.DependencyRegistratorBuilder<Basilisque.DependencyInjection.Tests.DependencyRegistrator> builder = new(services);
            });
        }
    }
}
