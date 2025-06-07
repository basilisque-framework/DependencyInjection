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
public class Register_1Class_With_FactoryMethodName_But_No_FactoryType_Shows_Error : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        /// <summary>
        /// Test class that will be registered with the factory method.
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceSingleton(FactoryMethodName = ""Create"")]
        public class MyPublicRegisteredClass
        {
        }
        ");
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        //error BAS_DI_002: The method name 'Create' of the factory was specified but the corresponding factory type is missing.
        yield return new Microsoft.CodeAnalysis.Testing.DiagnosticResult("BAS_DI_002", Microsoft.CodeAnalysis.DiagnosticSeverity.Error).WithSpan(5, 9, 8, 10);
    }
}

