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

#if NET8_0_OR_GREATER

using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.RegisterWithFactoryTests;

[TestClass]
public class Register_2Classes_As_Interface_With_Factory_When_Factory_Returns_Interface : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        using Microsoft.Extensions.DependencyInjection;

        /// <summary>
        /// Test factory that will be used to the create instances of the service.
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient(As = typeof(IMy), Factory = typeof(MyFactory))]
        public class MyFactory
        {
            /// <summary>
            /// Creates an instance of the service
            /// </summary>
            /// <param name=""serviceProvider"">The service provider</param>
            /// <returns>An instance of the service</returns>
            public static IMy Create(System.IServiceProvider serviceProvider)
            {
                var key = ""enabled""; // This could be dynamic based on some condition
                return serviceProvider.GetRequiredKeyedService<IMy>(key);
            }
        }
        ");

        sources.Add(@"
        /// <summary>
        /// Test interface
        /// </summary>
        public interface IMy
        {
        }
        ");

        sources.Add(@"
        /// <summary>
        /// Test class that will be registered with the factory method.
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient(As = typeof(IMy), Key = ""enabled"")]
        public class MyEnabled : IMy
        {
        }
        ");

        sources.Add(@"
        /// <summary>
        /// Test class that will be registered with the factory method.
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient(As = typeof(IMy), Key = ""disabled"")]
        public class MyDisabled : IMy
        {
        }
        ");
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddTransient<global::IMy>(global::MyFactory.Create);
        services.AddKeyedTransient<global::IMy, global::MyEnabled>(""enabled"");
        services.AddKeyedTransient<global::IMy, global::MyDisabled>(""disabled"");";
    }
}

#endif