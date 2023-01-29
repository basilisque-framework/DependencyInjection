namespace Basilisque.DependencyInjection.Registration.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    [RegisterService(RegistrationScope.Transient)]
    public class RegisterServiceTransientAttribute : RegisterServiceAttribute
    {
        public RegisterServiceTransientAttribute()
            : base(RegistrationScope.Transient)
        { }
    }
}
