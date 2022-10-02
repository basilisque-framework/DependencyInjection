using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.DependencyInjectionGeneratorTests
{
    [TestClass]
    public class NothingToRegister : BaseDependencyInjectionGeneratorTest
    {
        protected override void AddSourcesUnderTest(SourceFileList sources)
        {
            sources.Add(@"
        /// <summary>
        /// Test class that won't be registered to the dependency container
        /// </summary>
        public class MyPublicNotRegisteredClass
        {
        }
        ");
        }
    }
}

