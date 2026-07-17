/*
   Copyright 2023-2026 Alexander Stärk

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

using Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects;
using Basilisque.DependencyInjection.TestAssembly.TestObjects;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis;

public class Generated_IImplementsITypeName : BaseRegistrationTests
{
    [Test]
    public void Ensure_BaseInterface_IsService()
    {
        var isService = IsService<IImplementsITypeNameBase>();

        isService.ShouldBeTrue();
    }

    [Test]
    public void Ensure_Interface_IsService()
    {
        var isService = IsService<IImplementsITypeName>();

        isService.ShouldBeTrue();
    }

    [Test]
    public void Check_BaseInterface_Registered_Types()
    {
        var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeNameBase))).ToList();

        registeredServices.Count.ShouldBe(2);

        registeredServices[0].ImplementationType.ShouldBe(typeof(ImplementsITypeName));
        registeredServices[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);

        registeredServices[1].ImplementationType.ShouldBe(typeof(ImplementsITypeName2));
        registeredServices[1].Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Test]
    public void Check_Interface_Registered_Types()
    {
        var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeName))).ToList();

        registeredServices.Count.ShouldBe(1);

        registeredServices[0].ImplementationType.ShouldBe(typeof(ImplementsITypeName));
        registeredServices[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Test]
    public void Can_BaseInterface_Resolve_Instance()
    {
        var instances = Provider.GetServices<IImplementsITypeNameBase>().ToList();

        instances.Count.ShouldBe(2);

        instances[0].GetType().ShouldBe(typeof(ImplementsITypeName));
        instances[1].GetType().ShouldBe(typeof(ImplementsITypeName2));
    }

    [Test]
    public void Can_Interface_Resolve_Instance()
    {
        var instances = Provider.GetServices<IImplementsITypeName>().ToList();

        instances.Count.ShouldBe(1);

        instances[0].GetType().ShouldBe(typeof(ImplementsITypeName));
    }
}
