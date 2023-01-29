using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.Tests.TestObjects
{
    public interface IImplementsInterfaceDirectlyScoped
    {
    }

    [RegisterServiceScoped(As = typeof(IImplementsInterfaceDirectlyScoped), ImplementsITypeName = true)]
    internal class ImplementsInterfaceDirectlyScoped : IImplementsInterfaceDirectlyScoped
    {
    }
}
