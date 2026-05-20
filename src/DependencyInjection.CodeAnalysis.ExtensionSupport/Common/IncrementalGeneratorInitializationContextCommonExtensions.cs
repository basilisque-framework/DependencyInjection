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

namespace Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Common;

/// <summary>
/// Provides extension methods for configuring source output registration for Basilisque Dependency Injection extensions in an incremental generator initialization context.
/// </summary>
public static class IncrementalGeneratorInitializationContextCommonExtensions
{
    extension(IncrementalGeneratorInitializationContext context)
    {
        /// <summary>
        /// Creates a value provider that supplies the root namespace and assembly name for the current compilation.
        /// </summary>
        /// <returns>An incremental value provider that produces a tuple containing the root namespace and assembly name. Either
        /// value may be null if not specified in the project configuration.</returns>
        public IncrementalValueProvider<(string? RootNamespace, string? AssemblyName)> GetDependencyInjectionExtensionValueProvider()
        {
            var rootNamespaceProvider = context.AnalyzerConfigOptionsProvider.Select(DependencyInjectionGeneratorExtensionSelectors.RootNamespaceSelector);

            var assemblyNameProvider = context.CompilationProvider.Select(DependencyInjectionGeneratorExtensionSelectors.AssemblyNameSelector);

            var result = rootNamespaceProvider.Combine(assemblyNameProvider);

            return result;
        }
    }
}
