namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class DependencyInjectionGeneratorTests
    {
        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_Exists()
        {
            var registrator = new Basilisque.DependencyInjection.Tests.DependencyRegistrator();

            Assert.IsNotNull(registrator);
        }
    }
}
