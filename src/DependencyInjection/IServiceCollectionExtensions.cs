using Basilisque.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// This method extends <see cref="IServiceCollection"/> with a mechanism to register dependencies and services for the whole application.
        /// Calling this method creates a <see cref="DependencyRegistratorBuilder{TRootDependencyRegistrator}"/> and initializes the dependency chain.
        /// </summary>
        /// <typeparam name="TRootDependencyRegistrator"></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> all services are registered on.</param>
        /// <returns>A <see cref="DependencyRegistratorBuilder{TRootDependencyRegistrator}"/> that is used to build and execute the chain of <see cref="IDependencyRegistrator"/></returns>
        public static DependencyRegistratorBuilder<TRootDependencyRegistrator> InitializeDependencies<TRootDependencyRegistrator>(this IServiceCollection services)
            where TRootDependencyRegistrator : IDependencyRegistrator, new()
        {
            return new DependencyRegistratorBuilder<TRootDependencyRegistrator>(services);
        }
    }
}
