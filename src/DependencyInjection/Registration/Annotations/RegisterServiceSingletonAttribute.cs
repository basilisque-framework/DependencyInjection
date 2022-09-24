namespace Basilisque.DependencyInjection.Registration.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    [RegisterService(RegistrationScope.Singleton)]
    public class RegisterServiceSingletonAttribute : RegisterServiceAttribute
    {
        public RegisterServiceSingletonAttribute()
            : base(RegistrationScope.Singleton)
        { }
    }
}
