using Basilisque.DependencyInjection.Registration.Annotations;
using Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects;

namespace Basilisque.DependencyInjection.TestAssembly.TestObjects
{
    [RegisterServiceScoped(ImplementsITypeName = false)]
    [RegisterServiceSingleton(As = typeof(IMultipleRegistration), ImplementsITypeName = false)]
    [RegisterServiceTransient]
    public class MultipleRegistrations : IMultipleRegistration, IMultipleRegistrations
    {
    }
}
