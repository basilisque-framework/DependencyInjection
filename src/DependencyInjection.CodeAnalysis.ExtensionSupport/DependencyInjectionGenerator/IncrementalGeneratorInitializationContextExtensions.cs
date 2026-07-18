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
        /// Registers an output action for dependency injection extension compilation information using the specified input value provider and callback.
        /// </summary>
        /// <typeparam name="TSource">The type of the <see cref="Microsoft.CodeAnalysis.IncrementalValueProvider{T}"/> that provides the input values.</typeparam>
        /// <param name="valueProvider">An incremental value provider that supplies the root namespace, the assembly name and other input values for the compilation context.</param>
        /// <param name="extensionName">The name of the dependency injection extension for which the compilation information output is being registered. This is used to identify the specific extension in the output generation process.</param>
        /// <param name="initializeExtensionCallback">A callback that receives the implementation parameters and performs the initialization logic for the extension.</param>
        /// <param name="registerExtensionCallback">A callback that receives the implementation parameters and performs the registration logic for the extension.</param>
        public void RegisterDependencyInjectionExtensionCompilationInfoOutput<TSource>(IncrementalValueProvider<(TSource Left, (string? RootNamespace, string? AssemblyName) Right)> valueProvider, string extensionName, SourceGenerationImplementationCallback<TSource>? initializeExtensionCallback = null, SourceGenerationImplementationCallback<TSource>? registerExtensionCallback = null)
        {
            if (initializeExtensionCallback is null && registerExtensionCallback is null)
                throw new ArgumentNullException($"Please provide at least one of the parameters '{initializeExtensionCallback}' and '{registerExtensionCallback}'.");

            context.RegisterImplementationCompilationInfoOutput(valueProvider, (c, s, o) => DependencyInjectionExtensionGeneratorOutput.OutputImplementations(c, s, o, extensionName, initializeExtensionCallback, registerExtensionCallback));
        }

        /// <summary>
        /// Registers a dependency injection extension using the specified input value provider and callback.
        /// </summary>
        /// <typeparam name="TSource">The type of the <see cref="Microsoft.CodeAnalysis.IncrementalValueProvider{T}"/> that provides the input values.</typeparam>
        /// <param name="source">The <see cref="Microsoft.CodeAnalysis.IncrementalValueProvider{T}"/> that provides the input values.</param>
        /// <param name="extensionName">The name of the dependency injection extension for which the compilation information output is being registered. This is used to identify the specific extension in the output generation process.</param>
        /// <param name="initializeExtensionCallback">A callback that receives the implementation parameters and performs the initialization logic for the extension.</param>
        /// <param name="registerExtensionCallback">A callback that receives the implementation parameters and performs the registration logic for the extension.</param>
        public void CreateDependencyInjectionExtension<TSource>(IncrementalValueProvider<TSource> source, string extensionName, SourceGenerationImplementationCallback<TSource>? initializeExtensionCallback = null, SourceGenerationImplementationCallback<TSource>? registerExtensionCallback = null)
        {
            if (initializeExtensionCallback is null && registerExtensionCallback is null)
                throw new ArgumentNullException($"Please provide at least one of the parameters '{initializeExtensionCallback}' and '{registerExtensionCallback}'.");

            var valueProvider = context.GetDependencyInjectionExtensionValueProvider();

            var combinedProvider = source.Combine(valueProvider);

            context.RegisterDependencyInjectionExtensionCompilationInfoOutput(combinedProvider, extensionName, initializeExtensionCallback, registerExtensionCallback);
        }
    }
}
