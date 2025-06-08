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

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.KeyedRegistrationTests;

[TestClass]
public class Register_1Class_With_Key_Array_of_string : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        /// <summary>
        /// Test class that will be registered with the factory method.
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceScoped(Key = new string[] { ""a"", ""b"", ""c"" })]
        public class MyPublicRegisteredClass
        {
        }
        ");
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddKeyedScoped<global::MyPublicRegisteredClass>(new string[] { ""a"", ""b"", ""c"" });";
    }
}

#endif