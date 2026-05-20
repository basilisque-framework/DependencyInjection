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
    /// Generates and outputs dependency injection extension implementations using the specified parameters and callback.
    /// </summary>
    /// <remarks>This method checks preconditions and cancellation before proceeding. If preconditions are not
    /// met or cancellation is requested, the method returns without generating output.</remarks>
    /// <typeparam name="TParam">The type of the implementation parameters passed to the callback.</typeparam>
    /// <param name="sourceProductionContext">The context for source generation, used to report diagnostics and add generated source files.</param>
    /// <param name="provider">A tuple containing the root namespace and assembly name used to identify the main compilation target.</param>
    /// <param name="registrationOptions">The options that control how code generation is performed.</param>
    /// <param name="extensionName">The name of the extension to be generated.</param>
    /// <param name="implementationParameters">The parameters to be passed to the implementation callback for generating the extension.</param>
    /// <param name="implementationCallback">A callback that receives the implementation parameters and performs the actual implementation logic.</param>
    public static void OutputImplementations<TParam>(SourceProductionContext sourceProductionContext, (string? RootNamespace, string? AssemblyName) provider, Basilisque.CodeAnalysis.Syntax.RegistrationOptions registrationOptions, string extensionName, TParam implementationParameters, SourceGenerationImplementationCallback<TParam> implementationCallback)
    {
        sourceProductionContext.CancellationToken.ThrowIfCancellationRequested();

        if (!DependencyInjectionGeneratorOutputUtilities.CheckPreconditions(sourceProductionContext, provider, registrationOptions))
            return;

        (string mainCompilationName, string mainNamespace, _, _, _) = DependencyInjectionGeneratorOutputUtilities.GetMainCompilationTarget(provider);

        outputDependencyRegistratorExtensionImplementation(sourceProductionContext, registrationOptions, mainNamespace, mainCompilationName, extensionName, implementationParameters, implementationCallback);
    }

    private static void outputDependencyRegistratorExtensionImplementation<TParam>(SourceProductionContext context, RegistrationOptions registrationOptions, string mainNamespace, string mainCompilationName, string extensionName, TParam implementationParameters, SourceGenerationImplementationCallback<TParam> implementationCallback)
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

            var initializeMethod = new MethodInfo(true, $"initializeExtension_{extensionName}")
            {
                Parameters = {
                    new ParameterInfo(ParameterKind.Ordinary, "DependencyCollection", "collection")
                }
            };

            initializeMethod.Body.Add($@"/* register services for extension '{extensionName}' */");

            //addServicesToBody(context, registerServicesGeneratedMethod.Body, servicesToRegister);
            implementationCallback(context, implementationParameters);

            cl.Methods.Add(initializeMethod);
        })
            .AddToSourceProductionContext();
    }
}
