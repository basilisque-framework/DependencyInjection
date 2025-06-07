/*
   Copyright 2023-2025 Alexander Stärk

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

namespace Basilisque.DependencyInjection.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor MissingAssemblyName { get { return new DiagnosticDescriptor("BAS_DI_001", "The assembly name could not be determined.", "The name of the assembly is empty", "Basilisque.DependencyInjection", DiagnosticSeverity.Error, true); } }
    public static DiagnosticDescriptor FactoryTypeNotDefined { get { return new DiagnosticDescriptor("BAS_DI_002", "The factory type is not defined.", "The method name '{factoryMethodName}' of the factory was specified but the corresponding factory type is missing.", "Basilisque.DependencyInjection", DiagnosticSeverity.Error, true); } }
    public static DiagnosticDescriptor FactoryMethodNotFound { get { return new DiagnosticDescriptor("BAS_DI_003", "Could not determine the factory method.", "Could not determine the factory method. Please ensure the factory '{factoryTypeName}' contains a single method with the correct signature or provide the name of the method.", "Basilisque.DependencyInjection", DiagnosticSeverity.Error, true); } }
}
