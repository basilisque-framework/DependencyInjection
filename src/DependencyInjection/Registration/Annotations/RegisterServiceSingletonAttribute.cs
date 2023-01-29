namespace Basilisque.DependencyInjection.Registration.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    [RegisterService(RegistrationScope.Singleton)]
    public class RegisterServiceSingletonAttribute : RegisterServiceAttribute
    {
        public RegisterServiceSingletonAttribute()
            : base(RegistrationScope.Singleton)
        { }
    }
}
