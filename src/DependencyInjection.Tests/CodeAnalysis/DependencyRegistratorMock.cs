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

using Basilisque.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace Basilisque.DependencyInjection.Tests
{
    public partial class DependencyRegistrator
    {
        public List<string> MethodCalls = new();

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
