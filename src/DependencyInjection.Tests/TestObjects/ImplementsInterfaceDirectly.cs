using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.Tests.TestObjects
{
    public interface IImplementsInterfaceDirectly
    {
    }

    [RegisterServiceScoped(As = typeof(IImplementsInterfaceDirectly), ImplementsITypeName = true)]
    public class ImplementsInterfaceDirectly : IImplementsInterfaceDirectly
    {
    }
}
