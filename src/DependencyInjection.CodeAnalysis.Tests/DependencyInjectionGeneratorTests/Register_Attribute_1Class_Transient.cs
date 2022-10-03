﻿using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.DependencyInjectionGeneratorTests
{
    [TestClass]
    public class Register_Attribute_1Class_Transient : BaseDependencyInjectionGeneratorTest
    {
        protected override void AddSourcesUnderTest(SourceFileList sources)
        {
            sources.Add(@"
        /// <summary>
        /// Test class that will be registered as transient by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient]
        public class MyPublicRegisteredClass
        {
        }
        ");
        }

        protected override string? GetRegisteredServicesSource()
        {
            return @"
            services.AddTransient<MyPublicRegisteredClass>();";
        }
    }
}

