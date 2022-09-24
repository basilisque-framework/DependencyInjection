namespace Basilisque.DependencyInjection.Registration.Annotations
{
    public class RegisterServiceAttribute : Attribute, IRegisterServiceAttribute
    {
        public RegistrationScope Scope { get; protected set; }
        public Type? As { get; set; } = null;
        public bool ImplementsITypeName { get; set; } = true;

        public RegisterServiceAttribute(RegistrationScope scope)
        {
            Scope = scope;
        }
    }
}
