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
using Basilisque.DependencyInjection.Registration.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace Basilisque.DependencyInjection.CodeAnalysis.ExtensionSupport.Tests.Common;

[TestClass]
public class CodeLinesExtensionsTests
{
    private CodeLines _methodBody = new();

    [TestMethod]
    public void AddServicesRegistrationToMethodBody_AddsNonKeyedRegistration_WithImplementationType()
    {
        var serviceType = getTypeSymbol(
            "namespace My.Service; public interface IMyService { }",
            "My.Service.IMyService");
        var implementationType = getTypeSymbol(
            "namespace My.Service; public class MyService : IMyService { }",
            "My.Service.MyService");

        _methodBody.AddServicesRegistrationToMethodBody(RegistrationScope.Scoped, serviceType, implementationType);

        CollectionAssert.AreEqual(
            new[] { "services.AddScoped<global::My.Service.IMyService, global::My.Service.MyService>();" },
            _methodBody.ToArray());
    }

    [TestMethod]
    public void AddServicesRegistrationToMethodBody_AddsKeyedRegistration_WithFactoryInformation()
    {
        var serviceType = getTypeSymbol(
            "namespace My.Service; public class MyService { }",
            "My.Service.MyService");

        _methodBody.AddServicesRegistrationToMethodBody(RegistrationScope.Singleton, serviceType, serviceKey: "\"my-key\"", factoryInformation: "global::My.Factory.Create");

        CollectionAssert.AreEqual(
            new[] { "services.AddKeyedSingleton<global::My.Service.MyService>(\"my-key\", global::My.Factory.Create);" },
            _methodBody.ToArray());
    }

    [TestMethod]
    public void AddServicesRegistrationToMethodBody_AddsOpenGenericRegistration()
    {
        var serviceType = getTypeSymbol(
            "namespace My.Service; public interface IMyService<T> { }",
            "My.Service.IMyService`1")
            .ConstructUnboundGenericType();
        var implementationType = getTypeSymbol(
            "namespace My.Service; public class MyService<T> : IMyService<T> { }",
            "My.Service.MyService`1");

        _methodBody.AddServicesRegistrationToMethodBody(RegistrationScope.Transient, serviceType, implementationType);

        CollectionAssert.AreEqual(
            new[] { "services.AddTransient(typeof(global::My.Service.IMyService<>), typeof(global::My.Service.MyService<>));" },
            _methodBody.ToArray());
    }

    private static INamedTypeSymbol getTypeSymbol(string sourceCode, string metadataName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("CodeLinesExtensionsTests")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTree);

        var symbol = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(symbol);
        return symbol;
    }
}
