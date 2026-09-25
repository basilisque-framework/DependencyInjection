/*
   Copyright 2026 Alexander Stärk

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

[InheritsTests]
public class Register_1Class_As_Closed_Generic_Interface : BaseDependencyInjectionGeneratorTest
{
    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
        sources.Add(@"
        public interface IUserContext<T>
        {
        }

        public interface IUserContext
        {
        }
        ");

        sources.Add(@"
        [Basilisque.DependencyInjection.Registration.Annotations.RegisterServiceScoped(As = typeof(IUserContext<System.Guid>), ImplementsITypeName = false)]
        public sealed class UserContext : UserContext<System.Guid>, IUserContext
        {
        }

        public abstract class UserContext<T> : IUserContext<T>
        {
        }
        ");
    }

    protected override string? GetRegisteredServicesSource()
    {
        return @"
        services.AddScoped<global::IUserContext<global::System.Guid>, global::UserContext>();";
    }
}
