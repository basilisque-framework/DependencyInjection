using Basilisque.DependencyInjection.Registration.Annotations;

namespace Basilisque.DependencyInjection.Tests.Registration.Annotations
{
    [TestClass]
    public class RegisterServiceAttributeTests
    {
        [TestMethod]
        public void Default_Values_Are_Valid()
        {
            RegisterServiceAttribute a = new(0);

            Assert.AreEqual(0, (int)a.Scope);
            Assert.IsNull(a.As);
            Assert.IsTrue(a.ImplementsITypeName);
        }
    }
}
