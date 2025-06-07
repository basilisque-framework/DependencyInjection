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

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.RegisterWithFactoryTests;

[TestClass]
public class Register_1Class_With_Factory_And_Invalid_MethodName_Shows_Error : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        /// <summary>
        /// Test class that will be registered with the factory method.
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceSingleton(Factory = typeof(MyFactory), FactoryMethodName = ""Create2"")]
        public class MyPublicRegisteredClass
        {
        }
        ");

        sources.Add(@"
        /// <summary>
        /// Test factory that will be used to the create instances of the service.
        /// </summary>
        public class MyFactory
        {
            /// <summary>
            /// Creates an instance of the service
            /// </summary>
            /// <param name=""serviceProvider"">The service provider</param>
            /// <returns>An instance of the service</returns>
            public static MyPublicRegisteredClass Create(System.IServiceProvider serviceProvider)
            {
                return new MyPublicRegisteredClass();
            }
        }
        ");
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        //error BAS_DI_004: Could not find a method named 'Create2' on the factory type 'global::MyFactory' that matches the correct method signature for a factory method.
        yield return new Microsoft.CodeAnalysis.Testing.DiagnosticResult("BAS_DI_004", Microsoft.CodeAnalysis.DiagnosticSeverity.Error).WithSpan(5, 9, 8, 10);
    }
}

