using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.DependencyInjectionGeneratorTests
{
    [TestClass]
    public class NoRootNamespace : BaseDependencyInjectionGeneratorTest
    {
        protected override string? GetRootNamespace()
        {
            return null;
        }

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

