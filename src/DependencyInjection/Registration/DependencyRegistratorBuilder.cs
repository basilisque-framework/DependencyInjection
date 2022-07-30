namespace Basilisque.DependencyInjection.Registration
{
    /// <summary>
    /// This builder is used by the <see cref="IServiceCollectionExtensions.InitializeDependencies{TRootDependencyRegistrator}(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/> method to build up the chain of <see cref="IDependencyRegistrator"/> in a <see cref="DependencyCollection"/> and to execute the service registration.
    /// </summary>
    /// <typeparam name="TRootDependencyRegistrator">The root <see cref="IDependencyRegistrator"/>. This is typically the <see cref="IDependencyRegistrator"/> implementation in you startup project.</typeparam>
    public class DependencyRegistratorBuilder<TRootDependencyRegistrator>
        where TRootDependencyRegistrator : IDependencyRegistrator, new()
    {
        private Microsoft.Extensions.DependencyInjection.IServiceCollection _services;
        private DependencyCollection _dependencyCollection;

        /// <summary>
        /// Creates a new <see cref="DependencyRegistratorBuilder{TRootDependencyRegistrator}"/>
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> that all services are registered on.</param>
        /// <exception cref="ArgumentNullException">Thrown when a parameter is null</exception>
        public DependencyRegistratorBuilder(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            _services = services;
            _dependencyCollection = DependencyCollection.Create<TRootDependencyRegistrator>();
        }

        /// <summary>
        /// Executes the registration of all services in the already built up dependency chain.
        /// </summary>
        public void RegisterServices()
        {
            var registrators = _dependencyCollection.GetRegistrators();

            foreach (var registrator in registrators)
                registrator.RegisterServices(_services);
        }

        ///// <summary>
        ///// Provides the possibility to search and register dependencies and services at runtime.
        ///// </summary>
        ///// <param name="config">A callback to configure the registration process</param>
        ///// <returns>The current <see cref="DependencyRegistratorBuilder{TRootDependencyRegistrator}"/></returns>
        //public DependencyRegistratorBuilder<TRootDependencyRegistrator> AddRuntimeDependencies(Action<Registration.Runtime.IMainServiceFindContext> config)
        //{
        //    var ctx = new Runtime.ServiceFindContext(_services);

        //    config(ctx);

        //    return this;
        //}
    }
}
