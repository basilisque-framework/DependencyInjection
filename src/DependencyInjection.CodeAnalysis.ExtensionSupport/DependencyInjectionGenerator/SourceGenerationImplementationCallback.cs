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

namespace Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.DependencyInjectionGenerator;

/// <summary>
/// Represents a callback method that executes source generation logic using the specified context and implementation parameters.
/// </summary>
/// <remarks>Use this delegate to encapsulate custom source generation logic that requires both a production
/// context and user-defined parameters. The callback is typically invoked during incremental source generation workflows.</remarks>
/// <typeparam name="TParam">The type of the implementation parameters provided to the callback.</typeparam>
/// <param name="context">The context for source production, providing access to code generation APIs and reporting mechanisms.</param>
/// <param name="implementationParameters">The parameters that supply additional information or configuration for the source generation implementation.</param>
public delegate void SourceGenerationImplementationCallback<TParam>(SourceProductionContext context, TParam implementationParameters);
