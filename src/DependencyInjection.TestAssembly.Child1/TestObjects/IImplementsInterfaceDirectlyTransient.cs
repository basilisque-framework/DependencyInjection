using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects
{
    [RegisterServiceTransient(As = typeof(IImplementsInterfaceDirectlyTransient), ImplementsITypeName = false)]
    public interface IImplementsInterfaceDirectlyTransient
    {
    }
}
