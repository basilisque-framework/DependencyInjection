using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects
{
    [RegisterServiceSingleton(As = typeof(IImplementsITypeNameBase), ImplementsITypeName = true)]
    public interface IImplementsITypeNameBase
    {
    }
}
