using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.Tests.Registration.Annotations
{
    [TestClass]
    public class RegisterServiceTransientAttributeTests
    {
        [TestMethod]
        public void Scope_Is_Correct()
        {
            RegisterServiceTransientAttribute a = new();
            Assert.AreEqual(0, (int)a.Scope);
        }
    }
}
