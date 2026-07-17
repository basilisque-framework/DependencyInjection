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

public class Generated_MultipleRegistrations : BaseRegistrationTests
{
    [Test]
    public void Ensure_MultipleRegistrations_IsService()
    {
        var isService = IsService<MultipleRegistrations>();

        isService.ShouldBeTrue();
    }

    [Test]
    public void Ensure_IMultipleRegistration_IsService()
    {
        var isService = IsService<IMultipleRegistration>();

        isService.ShouldBeTrue();
    }

    [Test]
    public void Ensure_IMultipleRegistrations_IsService()
    {
        var isService = IsService<IMultipleRegistrations>();

        isService.ShouldBeTrue();
    }

    [Test]
    public void Check_MultipleRegistrations_Registered_Types()
    {
        var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(MultipleRegistrations))).ToList();

        registeredServices.Count.ShouldBe(1);

        registeredServices[0].ImplementationType.ShouldBe(typeof(MultipleRegistrations));
        registeredServices[0].Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Test]
    public void Check_IMultipleRegistration_Registered_Types()
    {
        var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IMultipleRegistration))).ToList();

        registeredServices.Count.ShouldBe(1);

        registeredServices[0].ImplementationType.ShouldBe(typeof(MultipleRegistrations));
        registeredServices[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Test]
    public void Check_IMultipleRegistrations_Registered_Types()
    {
        var registeredServices = ServiceCollection.Where(sd => sd.ServiceType.Equals(typeof(IMultipleRegistrations))).ToList();

        registeredServices.Count.ShouldBe(1);

        registeredServices[0].ImplementationType.ShouldBe(typeof(MultipleRegistrations));
        registeredServices[0].Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Test]
    public void Can_MultipleRegistrations_Resolve_Instance()
    {
        var instances = Provider.GetServices<MultipleRegistrations>().ToList();

        instances.Count.ShouldBe(1);

        instances[0].GetType().ShouldBe(typeof(MultipleRegistrations));
    }

    [Test]
    public void Can_IMultipleRegistration_Resolve_Instance()
    {
        var instances = Provider.GetServices<IMultipleRegistration>().ToList();

        instances.Count.ShouldBe(1);

        instances[0].GetType().ShouldBe(typeof(MultipleRegistrations));
    }

    [Test]
    public void Can_IMultipleRegistrations_Resolve_Instance()
    {
        var instances = Provider.GetServices<IMultipleRegistrations>().ToList();

        instances.Count.ShouldBe(1);

        instances[0].GetType().ShouldBe(typeof(MultipleRegistrations));
    }
}
