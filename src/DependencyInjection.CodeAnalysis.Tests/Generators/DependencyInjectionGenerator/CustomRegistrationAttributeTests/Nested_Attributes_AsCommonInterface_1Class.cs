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
public class Nested_Attributes_AsCommonInterface_1Class : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        /// <summary>
        /// Custom attribute that registers a service as Some.Namespace.IMyCommonInterface
        /// </summary>
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceSingleton(As = typeof(Some.Namespace.IMyCommonInterface))]
        public class RegisterAsCommonInterfaceAttribute : System.Attribute, Basilisque.DependencyInjection.Registration.Annotations.IRegisterServiceAttribute
        {
            /// <summary>
            /// Creates a new instance of the attribute
            /// </summary>
            public RegisterAsCommonInterfaceAttribute(Basilisque.DependencyInjection.Registration.Annotations.RegistrationScope scope)
            {
            }
        }
        ");

        sources.Add(@"
        namespace Some.Namespace
        {
            /// <summary>
            /// Test interface
            /// </summary>
            public interface IMyCommonInterface
            {
            }
        }");

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
        [RegisterAsCommonInterface(Basilisque.DependencyInjection.Registration.Annotations.RegistrationScope.Singleton)]
        public class MyPublicRegisteredClass : IMyPublicRegisteredClass, IMyCommonInterface
        {
        }");
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddSingleton<global::Some.Namespace.IMyCommonInterface, global::MyPublicRegisteredClass>();
        services.AddSingleton<global::Some.Namespace.IMyPublicRegisteredClass, global::MyPublicRegisteredClass>();";
    }
}

