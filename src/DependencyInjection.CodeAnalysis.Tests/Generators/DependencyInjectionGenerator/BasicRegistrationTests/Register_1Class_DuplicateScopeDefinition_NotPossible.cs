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
public class Register_1Class_DuplicateScopeDefinition_NotPossible : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        /// <summary>
        /// Test class that will be registered as transient by attribute
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient(Scope = Basilisque.DependencyInjection.Registration.Annotations.RegistrationScope.Singleton)]
        public class MyPublicRegisteredClass
        {
        }");
    }

    protected override IEnumerable<DiagnosticResult> GetExpectedDiagnostics()
    {
        //error CS0617: 'Scope' is not a valid named attribute argument. Named attribute arguments must be fields which are not readonly, static, or const, or read-write properties which are public and not static.
        yield return new Microsoft.CodeAnalysis.Testing.DiagnosticResult("CS0617", Microsoft.CodeAnalysis.DiagnosticSeverity.Error).WithSpan(5, 91, 5, 96);
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddTransient<MyPublicRegisteredClass>();";
    }
}

