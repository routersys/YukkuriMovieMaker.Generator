using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace YukkuriMovieMaker.Generator;

internal readonly struct GeneratorContextData : IEquatable<GeneratorContextData>
{
    internal INamedTypeSymbol Symbol { get; }
    internal SyntaxTokenList Modifiers { get; }
    internal string CsFilePath { get; }
    internal INamedTypeSymbol? AttributeSymbol { get; }
    internal string? CsvContent { get; }

    internal GeneratorContextData(
        INamedTypeSymbol symbol,
        SyntaxTokenList modifiers,
        string csFilePath,
        INamedTypeSymbol? attributeSymbol,
        string? csvContent)
    {
        Symbol = symbol;
        Modifiers = modifiers;
        CsFilePath = csFilePath;
        AttributeSymbol = attributeSymbol;
        CsvContent = csvContent;
    }

    public bool Equals(GeneratorContextData other) =>
        SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol) &&
        Modifiers.SequenceEqual(other.Modifiers) &&
        CsFilePath == other.CsFilePath &&
        SymbolEqualityComparer.Default.Equals(AttributeSymbol, other.AttributeSymbol) &&
        CsvContent == other.CsvContent;

    public override bool Equals(object? obj) =>
        obj is GeneratorContextData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        SymbolEqualityComparer.Default.GetHashCode(Symbol),
        CsFilePath,
        CsvContent,
        AttributeSymbol is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(AttributeSymbol));
}