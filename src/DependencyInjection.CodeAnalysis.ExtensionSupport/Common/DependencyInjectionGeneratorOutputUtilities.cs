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

namespace Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Common;

internal class DependencyInjectionGeneratorOutputUtilities
{
    internal static bool CheckPreconditions(SourceProductionContext context, (string? RootNamespace, string? AssemblyName) provider, RegistrationOptions registrationOptions)
    {
        //check preconditions
        if (registrationOptions.Language != Language.CSharp)
            throw new System.NotSupportedException($"The language '{registrationOptions.Language}' is currently not supported by this generator.");

        if (!checkPreconditionAssemblyName(context, provider.AssemblyName))
            return false;

        return true;
    }

    internal static (string mainCompilationName, string mainNamespace, bool hasRootNamespace, string? rootNamespace, string assemblyNameNamespace) GetMainCompilationTarget((string? RootNamespace, string? AssemblyName) provider)
    {
        //get the target namespace(s)
        var rootNamespace = provider.RootNamespace;
        var hasRootNamespace = !string.IsNullOrWhiteSpace(rootNamespace) && rootNamespace != provider.AssemblyName;

        var assemblyNameNamespace = provider.AssemblyName.ToValidNamespace()!;

        //check if the target project has a rootnamespace defined and if it differs from the assemblyname
        string mainCompilationName;
        string mainNamespace;
        if (hasRootNamespace)
        {
            //prepare to output the main registrator class in the rootnamespace
            mainCompilationName = DependencyInjectionGeneratorData.C_DEPENDENCY_REGISTRATOR_ROOTNAMESPACE_COMPILATIONNAME;
            mainNamespace = rootNamespace!;
        }
        else
        {
            //prepare to output the main registrator class in the assemblyname as namespace
            mainCompilationName = DependencyInjectionGeneratorData.C_DEPENDENCY_REGISTRATOR_ASSEMBLYNAMENAMESPACE_COMPILATIONNAME;
            mainNamespace = assemblyNameNamespace;
        }

        return (mainCompilationName, mainNamespace, hasRootNamespace, rootNamespace, assemblyNameNamespace);
    }

    private static bool checkPreconditionAssemblyName(SourceProductionContext context, string? assemblyName)
    {
        if (!string.IsNullOrEmpty(assemblyName))
            return true;

        var missingAssemblyNameDiagnostic = Diagnostic.Create(DiagnosticDescriptors.MissingAssemblyName, Location.None);
        context.ReportDiagnostic(missingAssemblyNameDiagnostic);

        return false;
    }
}
