﻿/*
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

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.CustomRegistrationAttributeTests;

[TestClass]
public class Register_1Class_Transient : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        /// <summary>
        /// Custom attribute that registers a service as scoped
        /// </summary>
        public class MyCustomScopedAttribute : System.Attribute, Basilisque.DependencyInjection.Registration.Annotations.IRegisterServiceAttribute
        {
            /// <summary>
            /// Creates a new instance of the attribute
            /// </summary>
            public MyCustomScopedAttribute(Basilisque.DependencyInjection.Registration.Annotations.RegistrationScope scope)
            {
            }
        }
        ");

        sources.Add(@"
        /// <summary>
        /// Test class that will be registered as transient by attribute
        /// </summary>
        [MyCustomScopedAttribute(Basilisque.DependencyInjection.Registration.Annotations.RegistrationScope.Scoped)]
        public class MyPublicRegisteredClass
        {
        }
        ");
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddScoped<global::MyPublicRegisteredClass>();";
    }
}

