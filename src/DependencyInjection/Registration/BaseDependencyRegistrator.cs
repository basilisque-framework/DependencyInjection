namespace Basilisque.DependencyInjection.Registration
{
    /// <summary>
    /// Provides a mechanism to register all dependencies and services of a project.
    /// </summary>
    public abstract class BaseDependencyRegistrator : IDependencyRegistrator
    {
        /// <summary>
        /// Registers all dependencies of the given project
        /// </summary>
        /// <param name="collection">The <see cref="DependencyCollection"/> that the dependencies will be registered on.</param>
        public void Initialize(DependencyCollection collection)
        {
            PerformInitialization(collection);
        }

        /// <summary>
        /// Registers all services of the given project
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> that the services will be registered on.</param>
        public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            PerformServiceRegistration(services);
        }

        /// <summary>
        /// Registers all dependencies of the given project
        /// </summary>
        /// <param name="collection">The <see cref="DependencyCollection"/> that the dependencies will be registered on.</param>
        protected abstract void PerformInitialization(DependencyCollection collection);

        /// <summary>
        /// Registers all services of the given project
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> that the services will be registered on.</param>
        protected abstract void PerformServiceRegistration(Microsoft.Extensions.DependencyInjection.IServiceCollection services);
    }
}
