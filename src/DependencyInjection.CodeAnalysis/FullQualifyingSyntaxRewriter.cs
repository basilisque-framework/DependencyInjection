using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Basilisque.DependencyInjection.CodeAnalysis;

/// <summary>
/// A syntax rewriter that replaces type references in C# syntax trees with their fully qualified names.
/// </summary>
/// <remarks>This rewriter traverses a C# syntax tree and ensures that all type references, including those in 
/// `typeof` expressions, array types, and initializer expressions, are replaced with their fully qualified names using
/// the `global::` prefix.</remarks>
public class FullQualifyingSyntaxRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel _semanticModel;
    private readonly SymbolDisplayFormat _fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType);

    /// <summary>
    /// Initializes a new instance of the <see cref="FullQualifyingSyntaxRewriter"/> class.
    /// </summary>
    /// <param name="semanticModel">The semantic model used to resolve type symbols.</param>
    public FullQualifyingSyntaxRewriter(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
    }

    /// <inheritdoc/>
    public override SyntaxNode? VisitTypeOfExpression(TypeOfExpressionSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;

        if (typeSymbol != null && !typeSymbol.IsErrorType())
        {
            var fullTypeName = typeSymbol.ToDisplayString(_fullyQualifiedFormat);
            var newTypeSyntax = SyntaxFactory.ParseTypeName(fullTypeName).WithTriviaFrom(node.Type);

            return node.WithType(newTypeSyntax);
        }

        return base.VisitTypeOfExpression(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
    {
        var newType = (ArrayTypeSyntax)Visit(node.Type);
        var newInitializer = (InitializerExpressionSyntax?)Visit(node.Initializer);

        return node
            .WithType(newType)
            .WithInitializer(newInitializer)
            .WithTriviaFrom(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
    {
        var newInitializer = (InitializerExpressionSyntax?)Visit(node.Initializer);

        if (newInitializer is not null)
            return node.WithInitializer(newInitializer).WithTriviaFrom(node);
        else
            return node.WithTriviaFrom(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
    {
        var expressions = node.Expressions.Select(expr => (ExpressionSyntax)Visit(expr)).ToList();

        var withSeparators = new List<SyntaxNodeOrToken>();
        for (int i = 0; i < expressions.Count; i++)
        {
            withSeparators.Add(expressions[i]);
            if (i < expressions.Count - 1)
            {
                withSeparators.Add(
                    SyntaxFactory.Token(SyntaxKind.CommaToken)
                        .WithTrailingTrivia(SyntaxFactory.Space)
                );
            }
        }

        var separated = SyntaxFactory.SeparatedList<ExpressionSyntax>(withSeparators);
        return node.WithExpressions(separated).WithTriviaFrom(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode? VisitArrayType(ArrayTypeSyntax node)
    {
        var typeSymbol = _semanticModel.GetTypeInfo(node.ElementType).Type;

        if (typeSymbol != null && !typeSymbol.IsErrorType())
        {
            var fullElementType = SyntaxFactory.ParseTypeName(
                typeSymbol.ToDisplayString(_fullyQualifiedFormat))
                .WithTriviaFrom(node.ElementType);

            return node.WithElementType(fullElementType);
        }

        return base.VisitArrayType(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var symbol = _semanticModel.GetSymbolInfo(node).Symbol;

        if (symbol != null && symbol.Kind is SymbolKind.Field or SymbolKind.Property)
        {
            var fullName = symbol.ToDisplayString(_fullyQualifiedFormat);

            var parsed = SyntaxFactory.ParseExpression(fullName).WithTriviaFrom(node);

            return parsed;
        }

        return base.VisitMemberAccessExpression(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text == "nameof")
        {
            if (node.ArgumentList.Arguments.FirstOrDefault()?.Expression is ExpressionSyntax expr)
            {
                var symbol = _semanticModel.GetSymbolInfo(expr).Symbol;
                if (symbol != null && symbol.Kind == SymbolKind.NamedType)
                {
                    var fullName = symbol.ToDisplayString(_fullyQualifiedFormat);
                    var stringLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(fullName)).WithTriviaFrom(node);
                    return stringLiteral;
                }
            }
        }

        return base.VisitInvocationExpression(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        var symbol = _semanticModel.GetSymbolInfo(node).Symbol;

        if (symbol is ITypeSymbol typeSymbol && !typeSymbol.IsErrorType())
        {
            var fullName = typeSymbol.ToDisplayString(_fullyQualifiedFormat);
            var parsed = SyntaxFactory.ParseTypeName(fullName).WithTriviaFrom(node);
            return parsed;
        }

        return base.VisitIdentifierName(node);
    }
}
