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

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.InheritedRegistrationTests;

[TestClass]
public class Register_2Classes_Inherited_From_Interface_With_BaseClass_As_ITypeName : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        namespace Some.Namespace
        {
            /// <summary>
            /// Test interface
            /// </summary>
            [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceTransient(As = typeof(ISomeInterface))]
            public interface ISomeInterface
            {
            }
        }");

        sources.Add(@"
        using Some.Namespace;

        /// <summary>
        /// Test class that will be registered as transient by attribute
        /// </summary>        
        public abstract class MyBaseClass : ISomeInterface
        {
        }");

        sources.Add(@"
        /// <summary>
        /// Test interface
        /// </summary>
        public interface IMyPublicRegisteredClass
        {
        }");

        sources.Add(@"
        /// <summary>
        /// Test class that will be registered as transient by attribute
        /// </summary>
        public class MyPublicRegisteredClass : MyBaseClass, IMyPublicRegisteredClass
        {
        }");
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddTransient<global::Some.Namespace.ISomeInterface, global::MyPublicRegisteredClass>();
        services.AddTransient<global::IMyPublicRegisteredClass, global::MyPublicRegisteredClass>();";
    }
}

