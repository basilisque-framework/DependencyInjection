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

namespace Basilisque.DependencyInjection.Tests.CodeAnalysis
{
    [TestClass]
    public class DependencyInjectionGeneratorTests
    {
        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_Exists_In_RootNamespace()
        {
            var registrator = new Basilisque.DependencyInjection.Tests.DependencyRegistrator();

            Assert.IsNotNull(registrator);
        }

        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_Exists_In_Namespace_Equals_AssemblyName()
        {
            var registrator = new DependencyInjection.DivergingRootNamespace.Tests.DependencyRegistrator();

            Assert.IsNotNull(registrator);
        }

        [TestMethod]
        public void Ensure_Generated_DependencyRegistrator_In_AssemblyName_InheritsFromRootNamespace()
        {
            _ = new DependencyInjection.DivergingRootNamespace.Tests.DependencyRegistrator();

            var isAssignable = typeof(Basilisque.DependencyInjection.Tests.DependencyRegistrator).IsAssignableFrom(typeof(DependencyInjection.DivergingRootNamespace.Tests.DependencyRegistrator));

            Assert.IsTrue(isAssignable);
        }
    }
}
