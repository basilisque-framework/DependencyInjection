/*
   Copyright 2025 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.ITypeNameRegistrationTests;

[TestClass]
public class Register_3Classes_Mixed_ITypeName : BaseDependencyInjectionGeneratorTest
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
        }");
        sources.Add(@"
        using Some.Namespace;

        /// <summary>
        /// Test class that will be registered as transient by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient]
        public class MyPublicRegisteredClass : IMyPublicRegisteredClass
        {
        }");

        sources.Add(@"
        namespace Some.Namespace
        {
            /// <summary>
            /// Test interface 2
            /// </summary>
            public interface IMyPublicRegisteredClass2
            {
            }
        }");
        sources.Add(@"
        using Some.Namespace;

        /// <summary>
        /// Test 2nd class that will be registered as singleton by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceSingleton]
        public class MyPublicRegisteredClass2 : IMyPublicRegisteredClass2
        {
        }");

        sources.Add(@"
        namespace Some.Namespace
        {
            /// <summary>
            /// Test interface 3
            /// </summary>
            public interface IMyPublicRegisteredClass3
            {
            }
        }");
        sources.Add(@"
        using Some.Namespace;

        /// <summary>
        /// Test 3rd class that will be registered as scoped by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceScoped]
        public class MyPublicRegisteredClass3 : IMyPublicRegisteredClass3
        {
        }");
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddTransient<Some.Namespace.IMyPublicRegisteredClass, MyPublicRegisteredClass>();
        services.AddSingleton<Some.Namespace.IMyPublicRegisteredClass2, MyPublicRegisteredClass2>();
        services.AddScoped<Some.Namespace.IMyPublicRegisteredClass3, MyPublicRegisteredClass3>();";
    }
}

