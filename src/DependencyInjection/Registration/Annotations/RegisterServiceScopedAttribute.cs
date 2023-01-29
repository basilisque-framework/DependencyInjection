namespace Basilisque.DependencyInjection.Registration.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    [RegisterService(RegistrationScope.Scoped)]
    public class RegisterServiceScopedAttribute : RegisterServiceAttribute
    {
        public RegisterServiceScopedAttribute()
            : base(RegistrationScope.Scoped)
        { }
    }
}
