using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.DependencyInjectionGeneratorTests
{
    [TestClass]
    public class Register_Attribute_1Class_Transient_ITypeName : BaseDependencyInjectionGeneratorTest
    {
        protected override void AddSourcesUnderTest(SourceFileList sources)
        {
            sources.Add(@"
        namespace Some.Namespace
        {
            /// <summary>
            /// Test interface
            /// </summary>
            public interface IMyPublicRegisteredClass
            {
            }
        }
");
            sources.Add(@"
        using Some.Namespace;

        /// <summary>
        /// Test class that will be registered as transient by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient]
        public class MyPublicRegisteredClass : IMyPublicRegisteredClass
        {
        }
        ");
        }

        protected override string? GetRegisteredServicesSource()
        {
            return @"
            services.AddTransient<Some.Namespace.IMyPublicRegisteredClass, MyPublicRegisteredClass>();";
        }
    }
}

