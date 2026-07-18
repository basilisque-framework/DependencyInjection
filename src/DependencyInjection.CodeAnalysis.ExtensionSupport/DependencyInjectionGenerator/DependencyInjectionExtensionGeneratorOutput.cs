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
/// Provides methods for generating and outputting dependency injection extension implementations during source generation.
/// </summary>
/// <remarks>This class is intended for use within source generators to facilitate the creation of extension
/// methods and related infrastructure for dependency injection registration. It is not intended to be used directly in
/// application code.</remarks>
public static class DependencyInjectionExtensionGeneratorOutput
{
    /// <summary>
    /// Generates and outputs dependency injection extension implementations using the specified input value provider and callback.
    /// </summary>
    /// <remarks>This method checks preconditions and cancellation before proceeding. If preconditions are not
    /// met or cancellation is requested, the method returns without generating output.</remarks>
    /// <typeparam name="TSource">The type of the input values.</typeparam>
    /// <param name="sourceProductionContext">The context for source generation, used to report diagnostics and add generated source files.</param>
    /// <param name="provider">A tuple containing the input values and a tuple with the root namespace and assembly name used to identify the main compilation target.</param>
    /// <param name="registrationOptions">The options that control how code generation is performed.</param>
    /// <param name="extensionName">The name of the extension to be generated.</param>
    /// <param name="initializeExtensionCallback">A callback that receives the implementation parameters and performs the initialization logic for the extension.</param>
    /// <param name="registerExtensionCallback">A callback that receives the implementation parameters and performs the registration logic for the extension.</param>
    public static void OutputImplementations<TSource>(SourceProductionContext sourceProductionContext, (TSource Left, (string? RootNamespace, string? AssemblyName) Right) provider, Basilisque.CodeAnalysis.Syntax.RegistrationOptions registrationOptions, string extensionName, SourceGenerationImplementationCallback<TSource>? initializeExtensionCallback = null, SourceGenerationImplementationCallback<TSource>? registerExtensionCallback = null)
    {
        sourceProductionContext.CancellationToken.ThrowIfCancellationRequested();

        if (initializeExtensionCallback is null && registerExtensionCallback is null)
            throw new ArgumentNullException($"Please provide at least one of the parameters '{initializeExtensionCallback}' and '{registerExtensionCallback}'.");

        if (!DependencyInjectionGeneratorOutputUtilities.CheckPreconditions(sourceProductionContext, provider.Right, registrationOptions))
            return;

        (string mainCompilationName, string mainNamespace, _, _, _) = DependencyInjectionGeneratorOutputUtilities.GetMainCompilationTarget(provider.Right);

        outputDependencyRegistratorExtensionImplementation(sourceProductionContext, registrationOptions, mainNamespace, mainCompilationName, extensionName, provider.Left, initializeExtensionCallback, registerExtensionCallback);
    }

    private static void outputDependencyRegistratorExtensionImplementation<TParam>(SourceProductionContext context, RegistrationOptions registrationOptions, string mainNamespace, string mainCompilationName, string extensionName, TParam implementationParameters, SourceGenerationImplementationCallback<TParam>? initializeExtensionCallback, SourceGenerationImplementationCallback<TParam>? registerExtensionCallback)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        //output implementation of the registrator class implementation for the extension
        var ci = registrationOptions.CreateCompilationInfo($"{mainCompilationName}.{extensionName}.impl", mainNamespace);
        ci.AddGeneratedCodeAttributes = false;
        ci.Usings.Add("Basilisque.DependencyInjection.Registration");
        ci.Usings.Add("Microsoft.Extensions.DependencyInjection");
        ci.AddNewClassInfo(DependencyInjectionGeneratorData.C_DEPENDENCY_REGISTRATOR_CLASSNAME, AccessModifier.Public, cl =>
        {
            cl.IsPartial = true;

            if (initializeExtensionCallback is not null)
            {
                var initializeMethod = getInitializeExtensionMethod(extensionName);
                initializeExtensionCallback(context, initializeMethod.Body, implementationParameters);
                cl.Methods.Add(initializeMethod);
            }

            if (registerExtensionCallback is not null)
            {
                var registerMethod = getRegisterExtensionMethod(extensionName);
                registerExtensionCallback(context, registerMethod.Body, implementationParameters);
                cl.Methods.Add(registerMethod);
            }
        })
            .AddToSourceProductionContext();
    }

    private static MethodInfo getInitializeExtensionMethod(string extensionName)
    {
        var initializeMethod = new MethodInfo(true, $"initializeExtension_{extensionName}")
        {
            Parameters = {
                    new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
                }
        };

        initializeMethod.Body.Add($@"/* initialize extension '{extensionName}' */");

        return initializeMethod;
    }

    private static MethodInfo getRegisterExtensionMethod(string extensionName)
    {
        var initializeMethod = new MethodInfo(true, $"registerExtension_{extensionName}")
        {
            Parameters = {
                    new ParameterInfo(ParameterKind.Ordinary, "IServiceCollection", "services")
                }
        };

        initializeMethod.Body.Add($@"/* register services for extension '{extensionName}' */");

        return initializeMethod;
    }
}
