/*
   Copyright 2023 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace Basilisque.DependencyInjection.Registration
{
    /// <summary>
    /// The <see cref="DependencyCollection"/> is used by the <see cref="DependencyRegistratorBuilder{TRootDependencyRegistrator}"/> to build a dependency chain of <see cref="IDependencyRegistrator"/>.
    /// To create an instance of <see cref="DependencyCollection"/> call the <see cref="Create{TRootDependencyRegistrator}"/> method.
    /// </summary>
    public class DependencyCollection
    {
        private Dictionary<Type, IDependencyRegistrator> _dependencyRegistrators = new Dictionary<Type, IDependencyRegistrator>();

        /// <summary>
        /// The constructor is private, because it should not be called directly.
        /// Please call the <see cref="Create{TRootDependencyRegistrator}"/> method instead.
        /// </summary>
        private DependencyCollection()
        { }

        /// <summary>
        /// Creates a new <see cref="DependencyCollection"/> and builds up the dependency chain of <see cref="IDependencyRegistrator"/> by adding the root <see cref="IDependencyRegistrator"/> to the collection.
        /// </summary>
        /// <typeparam name="TRootDependencyRegistrator">The root <see cref="IDependencyRegistrator"/>. This is typically the <see cref="IDependencyRegistrator"/> implementation in you startup project.</typeparam>
        /// <returns>A new <see cref="DependencyCollection"/> containing an instance of the provided <typeparamref name="TRootDependencyRegistrator"/>.</returns>
        internal static DependencyCollection Create<TRootDependencyRegistrator>()
            where TRootDependencyRegistrator : IDependencyRegistrator, new()
        {
            var result = new DependencyCollection();

            result.AddDependency<TRootDependencyRegistrator>();

            return result;
        }

        /// <summary>
        /// Adds a <see cref="IDependencyRegistrator"/> as dependency of the calling <see cref="IDependencyRegistrator"/> and initializes it.
        /// </summary>
        /// <typeparam name="TDependency">The <see cref="IDependencyRegistrator"/> that the calling <see cref="IDependencyRegistrator"/> depends on.</typeparam>
        public void AddDependency<TDependency>()
            where TDependency : IDependencyRegistrator, new()
        {
            //nothing to do when the dependency is already in the list
            if (_dependencyRegistrators.ContainsKey(typeof(TDependency)))
                return;

            //create a new instance of the registrator and add it to the list
            var registrator = new TDependency();
            _dependencyRegistrators.Add(typeof(TDependency), registrator);

            //initialize the registrator
            registrator.Initialize(this);
        }

        /// <summary>
        /// Returns all registered <see cref="IDependencyRegistrator"/>
        /// </summary>
        /// <returns>An <see cref="IEnumerable{IDependencyRegistrator}"/> containing all registered <see cref="IDependencyRegistrator"/></returns>
        internal IEnumerable<IDependencyRegistrator> GetRegistrators()
        {
            return _dependencyRegistrators.Values;
        }
    }
}
