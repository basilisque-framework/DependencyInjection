using Basilisque.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests
{
    public partial class DependencyRegistrator
    {
        public List<string> MethodCalls = new List<string>();

        partial void doBeforeInitialization(DependencyCollection collection)
        {
            MethodCalls.Add(nameof(doBeforeInitialization));
        }

        partial void doBeforeRegistration(IServiceCollection services)
        {
            services.AddSingleton(typeof(List<string>), MethodCalls);

            MethodCalls.Add(nameof(doBeforeRegistration));
        }

        partial void doAfterInitialization(DependencyCollection collection)
        {
            MethodCalls.Add(nameof(doAfterInitialization));
        }

        partial void doAfterRegistration(IServiceCollection services)
        {
            MethodCalls.Add(nameof(doAfterRegistration));
        }
    }
}
