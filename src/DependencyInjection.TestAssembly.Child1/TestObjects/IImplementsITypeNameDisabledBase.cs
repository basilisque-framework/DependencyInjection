using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects
{
    [RegisterServiceSingleton(As = typeof(IImplementsITypeNameDisabledBase), ImplementsITypeName = false)]
    public interface IImplementsITypeNameDisabledBase
    {
    }
}
