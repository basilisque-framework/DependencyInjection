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

using Basilisque.DependencyInjection.TestAssembly.Child1.TestObjects;

namespace Basilisque.DependencyInjection.TestAssembly.TestObjects
{
    /// <summary>
    /// Interface for testing the registration when the implementing class has the same name without the leading I
    /// </summary>
    public interface IImplementsITypeName2 : IImplementsITypeNameBase
    {
    }

    /// <summary>
    /// Class for testing the registration when the implemented interface has the same name with a leading I
    /// </summary>
    public class ImplementsITypeName2 : IImplementsITypeName2
    {
    }
}
