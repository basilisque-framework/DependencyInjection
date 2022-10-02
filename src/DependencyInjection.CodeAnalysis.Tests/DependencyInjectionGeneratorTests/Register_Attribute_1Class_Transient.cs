using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.DependencyInjectionGeneratorTests
{
    [TestClass]
    public class Register_Attribute_1Class_Transient : BaseDependencyInjectionGeneratorTest
    {
        protected override void AddSourcesUnderTest(SourceFileList sources)
        {
            sources.Add(@"
        /// <summary>
        /// Test interface that will be registered as transient by the attribute
        /// </summary>
        public interface IMyPublicNotRegisteredClass
        {
        }
");
            sources.Add(@"
        /// <summary>
        /// Test class that will be registered as transient by the attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient]
        public class MyPublicNotRegisteredClass : IMyPublicNotRegisteredClass
        {
        }
        ");
        }

        protected override string? GetRegisteredServicesSource()
        {
            return @"
            services.AddTransient<IMyPublicNotRegisteredClass, MyPublicNotRegisteredClass>();";
        }
    }
}

