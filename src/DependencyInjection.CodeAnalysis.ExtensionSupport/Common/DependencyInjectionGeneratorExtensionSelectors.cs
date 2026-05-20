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

using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading;

namespace Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Common;

internal class DependencyInjectionGeneratorExtensionSelectors
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "For debuging purposes")]
    internal static string? RootNamespaceSelector(AnalyzerConfigOptionsProvider provider, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? rootNamespace))
        {
            //#if DEBUG
            //                if (!System.Diagnostics.Debugger.IsAttached && rootNamespace == "Basilisque.DependencyInjection.Tests")
            //                {
            //                    System.Diagnostics.Debugger.Launch();
            //                }
            //#endif

            return rootNamespace;
        }
        else
            return null;
    }

    internal static string? AssemblyNameSelector(Compilation compilation, CancellationToken cancellationToken)
    {
        return compilation.AssemblyName;
    }
}
