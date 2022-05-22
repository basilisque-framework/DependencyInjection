namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class DependencyInjectionGeneratorTests
    {
        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_Exists_In_RootNamespace()
        {
            var registrator = new Basilisque.DependencyInjection.Tests.DependencyRegistrator();

            Assert.IsNotNull(registrator);
        }

        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_Exists_In_Namespace_Equals_AssemblyName()
        {
            var registrator = new DependencyInjection.DivergingRootNamespace.Tests.DependencyRegistrator();

            Assert.IsNotNull(registrator);
        }

        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_In_AssemblyName_InheritsFromRootNamespace()
        {
            var registrator = new DependencyInjection.DivergingRootNamespace.Tests.DependencyRegistrator();

            var isAssignable = typeof(Basilisque.DependencyInjection.Tests.DependencyRegistrator).IsAssignableFrom(typeof(DependencyInjection.DivergingRootNamespace.Tests.DependencyRegistrator));

            Assert.IsTrue(isAssignable);
        }
    }
}
