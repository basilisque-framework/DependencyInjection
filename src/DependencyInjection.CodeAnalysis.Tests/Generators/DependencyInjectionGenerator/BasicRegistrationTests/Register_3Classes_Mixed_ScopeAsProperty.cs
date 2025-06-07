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

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.BasicRegistrationTests;

[TestClass]
public class Register_3Classes_Mixed_ScopeAsProperty : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        /// <summary>
        /// Test class that throw an error because the attribute is missing its constructor argument
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterService]
        public class MyPublicRegisteredClass
        {
        }
        ");
        sources.Add(@"
        /// <summary>
        /// Test 2nd class that will be registered as scoped by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterService(Basilisque.DependencyInjection.Registration.Annotations.RegistrationScope.Scoped)]
        public class MyPublicRegisteredClass2
        {
        }
        ");
        sources.Add(@"
        namespace My.Test.NameSpace;
        /// <summary>
        /// Test 3rd class that will be registered as transient by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterService(Basilisque.DependencyInjection.Registration.Annotations.RegistrationScope.Transient)]
        public class MyPublicRegisteredClass3
        {
        }
        ");
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        //error CS7036: There is no argument given that corresponds to the required parameter 'scope' of 'RegisterServiceAttribute.RegisterServiceAttribute(RegistrationScope)'
        yield return new Microsoft.CodeAnalysis.Testing.DiagnosticResult("CS7036", Microsoft.CodeAnalysis.DiagnosticSeverity.Error).WithSpan(5, 10, 5, 81);
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddScoped<MyPublicRegisteredClass2>();
        services.AddTransient<My.Test.NameSpace.MyPublicRegisteredClass3>();";
    }
}

