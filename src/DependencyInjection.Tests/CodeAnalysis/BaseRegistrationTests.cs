using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    public class BaseRegistrationTests
    {
        private IServiceProviderIsService? _isService;
        private IServiceCollection? _serviceCollection;
        private ServiceProvider? _provider;

        protected IServiceCollection ServiceCollection { get { return _serviceCollection!; } }
        protected ServiceProvider Provider { get { return _provider!; } }

        [TestInitialize]
        public void Initialize()
        {
            _serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            ServiceCollection
                .InitializeDependencies<Basilisque.DependencyInjection.Tests.DependencyRegistrator>()
                .RegisterServices();

            _provider = ServiceCollection.BuildServiceProvider();
            _isService = Provider.GetService<IServiceProviderIsService>()!;
        }

        protected bool IsService<TService>()
        {
            return IsService(typeof(TService));
        }

        protected bool IsService(Type serviceType)
        {
            return _isService!.IsService(serviceType);
        }
    }
}
