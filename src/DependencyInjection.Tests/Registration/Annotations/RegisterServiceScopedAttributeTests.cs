using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.Tests.Registration.Annotations
{
    [TestClass]
    public class RegisterServiceScopedAttributeTests
    {
        [TestMethod]
        public void Scope_Is_Correct()
        {
            RegisterServiceScopedAttribute a = new();
            Assert.AreEqual(1, (int)a.Scope);
        }
    }
}
