/*
   Copyright 2025-2026 Alexander Stärk

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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests;

public class FullQualifyingSyntaxRewriterTests
{
    [Test]
    public void Rewrites_Typeof_ToCSAlias()
    {
        var code = "using System; class C { object o = typeof(String); }";
        var result = rewrite(code);
        result.ShouldContain("typeof(string)");
    }

    [Test]
    public void Rewrites_ArrayType_ToCSAlias()
    {
        var code = "using System; class C { Object o = new String[] { \"a\" }; }";
        var result = rewrite(code);
        result.ShouldContain("object");
        result.ShouldContain("string[]");
    }

    [Test]
    public void Rewrites_Nameof_ToFullyQualifiedString()
    {
        var code = "class MyType { string s = nameof(MyType); }";
        var result = rewrite(code);
        result.ShouldContain("\"global::MyType\"");
    }

    [Test]
    public void Preserves_Other_Syntax_Unchanged()
    {
        var code = "class C { int x = 42; }";
        var result = rewrite(code);
        result.ShouldContain("int x = 42;");
    }

    [Test]
    public void Handles_ImplicitArrayCreation_And_CustomClasses_WithTypeof()
    {
        var code = "using My.Namespace; class C { var types = new[] { typeof(A) }; }";
        var code2 = "namespace My.Namespace; class A { }";
        var result = rewrite(code, code2);
        result.ShouldContain("global::My.Namespace.A");
    }

    private static string rewrite(string sourceCode, string? sourceCode2 = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            .AddSyntaxTrees(syntaxTree);

        if (sourceCode2 is not null)
        {
            var syntaxTree2 = CSharpSyntaxTree.ParseText(sourceCode2);
            compilation = compilation.AddSyntaxTrees(syntaxTree2);
        }

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var rewriter = new FullQualifyingSyntaxRewriter(semanticModel);
        var root = syntaxTree.GetRoot();
        var newRoot = rewriter.Visit(root);
        return newRoot?.ToFullString() ?? string.Empty;
    }
}
