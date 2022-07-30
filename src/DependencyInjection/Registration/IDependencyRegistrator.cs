namespace Basilisque.DependencyInjection.Registration
{
    /// <summary>
    /// Implementors of this interface provide a mechanism to register all dependencies and services of a project.
    /// </summary>
    public interface IDependencyRegistrator
    {
        /// <summary>
        /// Registers all dependencies of the given project
        /// </summary>
        /// <param name="collection">The <see cref="DependencyCollection"/> that the dependencies will be registered on.</param>
        void Initialize(DependencyCollection collection);

        /// <summary>
        /// Registers all services of the given project
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> that the services will be registered on.</param>
        void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services);
    }
}
