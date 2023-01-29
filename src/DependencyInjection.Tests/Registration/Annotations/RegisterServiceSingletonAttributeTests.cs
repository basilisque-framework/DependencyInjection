using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.Tests.Registration.Annotations
{
    [TestClass]
    public class RegisterServiceSingletonAttributeTests
    {
        [TestMethod]
        public void Scope_Is_Correct()
        {
            RegisterServiceSingletonAttribute a = new();
            Assert.AreEqual(2, (int)a.Scope);
        }
    }
}
