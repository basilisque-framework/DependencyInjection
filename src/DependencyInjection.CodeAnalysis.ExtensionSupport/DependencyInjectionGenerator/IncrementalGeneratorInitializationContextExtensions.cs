/*
   Copyright 2026 Alexander Stärk

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

using Basilisque.CodeAnalysis.Syntax;
using Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Common;

namespace Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.DependencyInjectionGenerator;

/// <summary>
/// Provides extension methods for configuring source output registration for Basilisque Dependency Injection Extensions in an incremental generator initialization context.
/// </summary>
public static class IncrementalGeneratorInitializationContextExtensions
{
    extension(IncrementalGeneratorInitializationContext context)
    {
        /// <summary>
        /// Registers an output action for dependency injection extension compilation information using the specified
        /// value provider, implementation parameters, and callback.
        /// </summary>
        /// <typeparam name="TParam">The type of the implementation parameters to be passed to the callback.</typeparam>
        /// <param name="valueProvider">An incremental value provider that supplies the root namespace and assembly name for the compilation context.</param>
        /// <param name="extensionName">The name of the dependency injection extension for which the compilation information output is being registered. This is used to identify the specific extension in the output generation process.</param>
        /// <param name="implementationParameters">The parameters to be provided to the implementation callback when generating the output.</param>
        /// <param name="implementationCallback">A callback action that receives the implementation parameters and is invoked during output generation.</param>
        public void RegisterDependencyInjectionExtensionCompilationInfoOutput<TParam>(IncrementalValueProvider<(string? RootNamespace, string? AssemblyName)> valueProvider, string extensionName, TParam implementationParameters, SourceGenerationImplementationCallback<TParam> implementationCallback)
        {
            context.RegisterImplementationCompilationInfoOutput(valueProvider, (c, s, o) => DependencyInjectionExtensionGeneratorOutput.OutputImplementations(c, s, o, extensionName, implementationParameters, implementationCallback));
        }

        /// <summary>
        /// Registers a dependency injection extension using the specified implementation parameters and callback.
        /// </summary>
        /// <typeparam name="TParam">The type of the implementation parameters used to configure the dependency injection extension.</typeparam>
        /// <param name="extensionName">The name of the dependency injection extension for which the compilation information output is being registered. This is used to identify the specific extension in the output generation process.</param>
        /// <param name="implementationParameters">The parameters used to configure the dependency injection extension. These provide the necessary information for the extension's setup.</param>
        /// <param name="implementationCallback">A callback that receives the implementation parameters and performs additional configuration for the dependency injection extension.</param>
        public void CreateDependencyInjectionExtension<TParam>(string extensionName, TParam implementationParameters, SourceGenerationImplementationCallback<TParam> implementationCallback)
        {
            var valueProvider = context.GetDependencyInjectionExtensionValueProvider();

            context.RegisterDependencyInjectionExtensionCompilationInfoOutput(valueProvider, extensionName, implementationParameters, implementationCallback);
        }
    }
}
