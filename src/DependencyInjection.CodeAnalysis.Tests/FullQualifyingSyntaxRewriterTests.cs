using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests;

[TestClass]
public class FullQualifyingSyntaxRewriterTests
{
    [TestMethod]
    public void Rewrites_Typeof_ToCSAlias()
    {
        var code = "using System; class C { object o = typeof(String); }";
        var result = rewrite(code);
        StringAssert.Contains(result, "typeof(string)");
    }

    [TestMethod]
    public void Rewrites_ArrayType_ToCSAlias()
    {
        var code = "using System; class C { Object o = new String[] { \"a\" }; }";
        var result = rewrite(code);
        StringAssert.Contains(result, "object");
        StringAssert.Contains(result, "string[]");
    }

    [TestMethod]
    public void Rewrites_Nameof_ToFullyQualifiedString()
    {
        var code = "class MyType { string s = nameof(MyType); }";
        var result = rewrite(code);
        StringAssert.Contains(result, "\"global::MyType\"");
    }

    [TestMethod]
    public void Preserves_Other_Syntax_Unchanged()
    {
        var code = "class C { int x = 42; }";
        var result = rewrite(code);
        StringAssert.Contains(result, "int x = 42;");
    }

    [TestMethod]
    public void Handles_ImplicitArrayCreation_And_CustomClasses_WithTypeof()
    {
        var code = "using My.Namespace; class C { var types = new[] { typeof(A) }; }";
        var code2 = "namespace My.Namespace; class A { }";
        var result = rewrite(code, code2);
        StringAssert.Contains(result, "global::My.Namespace.A");
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
