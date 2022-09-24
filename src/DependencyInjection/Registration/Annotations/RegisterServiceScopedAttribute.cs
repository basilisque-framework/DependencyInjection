namespace Basilisque.DependencyInjection.Registration.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    [RegisterService(RegistrationScope.Scoped)]
    public class RegisterServiceScopedAttribute : RegisterServiceAttribute
    {
        public RegisterServiceScopedAttribute()
            : base(RegistrationScope.Scoped)
        { }
    }
}
