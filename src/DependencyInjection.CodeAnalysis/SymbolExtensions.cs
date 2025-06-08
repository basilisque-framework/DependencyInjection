namespace Basilisque.DependencyInjection.CodeAnalysis;

/// <summary>
/// Provides extension methods for <see cref="ITypeSymbol"/>.
/// </summary>
public static class SymbolExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="ITypeSymbol"/> represents an error type.
    /// </summary>
    /// <param name="symbol">The <see cref="ITypeSymbol"/> to evaluate. Must not be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <paramref name="symbol"/> represents an error type; otherwise, <see langword="false"/>.</returns>
    public static bool IsErrorType(this ITypeSymbol symbol)
    {
        return symbol.TypeKind == TypeKind.Error;
    }
}
