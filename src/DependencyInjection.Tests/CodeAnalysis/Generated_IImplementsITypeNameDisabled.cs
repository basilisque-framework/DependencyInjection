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

public class Generated_IImplementsITypeNameDisabled : BaseRegistrationTests
{
    [Test]
    public void Ensure_BaseInterface_IsService()
    {
        var isService = IsService<IImplementsITypeNameDisabledBase>();

        isService.ShouldBeTrue();
    }

    [Test]
    public void Ensure_Interface_IsService()
    {
        var isService = IsService<IImplementsITypeNameDisabled>();

        isService.ShouldBeFalse();
    }

    [Test]
    public void Check_BaseInterface_Registered_Types()
    {
        var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeNameDisabledBase))).ToList();

        registeredServices.Count.ShouldBe(2);

        registeredServices[0].ImplementationType.ShouldBe(typeof(ImplementsITypeNameDisabled));
        registeredServices[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);

        registeredServices[1].ImplementationType.ShouldBe(typeof(ImplementsITypeNameDisabled2));
        registeredServices[1].Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Test]
    public void Check_Interface_Registered_Types()
    {
        var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IImplementsITypeNameDisabled))).ToList();

        registeredServices.Count.ShouldBe(0);
    }

    [Test]
    public void Can_BaseInterface_Resolve_Instance()
    {
        var instances = Provider.GetServices<IImplementsITypeNameDisabledBase>().ToList();

        instances.Count.ShouldBe(2);

        instances[0].GetType().ShouldBe(typeof(ImplementsITypeNameDisabled));
        instances[1].GetType().ShouldBe(typeof(ImplementsITypeNameDisabled2));
    }

    [Test]
    public void Can_Interface_Resolve_Instance()
    {
        var instances = Provider.GetServices<IImplementsITypeNameDisabled>().ToList();

        instances.Count.ShouldBe(0);
    }
}
