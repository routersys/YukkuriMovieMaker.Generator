using CsvHelper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace YukkuriMovieMaker.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class AutoGenLocalizerPluginGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "AutoGenLocalizerAttribute.g.cs",
            SourceTextBuilder.CreateAttributeSource()));

        var classProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, token) => TransformToClassInfo(ctx, token))
            .Where(static info => info.Symbol is not null);

        var attributeSymbolProvider = context.CompilationProvider
            .Select(static (compilation, _) =>
                compilation.GetTypeByMetadataName("YukkuriMovieMaker.Generator.AutoGenLocalizerAttribute"));

        var csvProvider = context.AdditionalTextsProvider
            .Where(static x => x.Path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .Select(static (text, token) => (
                FileName: Path.GetFileName(text.Path),
                Content: text.GetText(token)?.ToString() ?? string.Empty))
            .Collect();

        var combinedProvider = classProvider
            .Combine(attributeSymbolProvider)
            .Combine(csvProvider)
            .Select(static (tuple, _) =>
            {
                var ((classInfo, attributeSymbol), csvFiles) = tuple;
                var targetCsvName = $"{Path.GetFileNameWithoutExtension(classInfo.CsFilePath)}.csv";
                var csvContent = csvFiles
                    .Where(x => x.FileName.Equals(targetCsvName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Content)
                    .FirstOrDefault();
                return new GeneratorContextData(classInfo.Symbol!, classInfo.Modifiers, classInfo.CsFilePath, attributeSymbol, csvContent);
            });

        context.RegisterSourceOutput(combinedProvider, GenerateSourceAndResx);
    }

    private static (INamedTypeSymbol? Symbol, SyntaxTokenList Modifiers, string CsFilePath) TransformToClassInfo(
        GeneratorSyntaxContext context,
        CancellationToken token)
    {
        if (context.Node is not ClassDeclarationSyntax classDecl)
            return (null, default, string.Empty);

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl, token);
        if (symbol is null)
            return (null, default, string.Empty);

        if (!classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            return (null, default, string.Empty);

        var filePath = context.Node.SyntaxTree.FilePath;
        if (string.IsNullOrEmpty(filePath))
            return (null, default, string.Empty);

        return (symbol, classDecl.Modifiers, filePath);
    }

    private static void GenerateSourceAndResx(SourceProductionContext context, GeneratorContextData data)
    {
        if (data.AttributeSymbol is null || !HasTargetAttribute(data.Symbol, data.AttributeSymbol))
            return;

        var targetCsvName = $"{Path.GetFileNameWithoutExtension(data.CsFilePath)}.csv";

        if (string.IsNullOrEmpty(data.CsvContent))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.FileNotFound, Location.None, targetCsvName));
            return;
        }

        var records = ReadCsvRecords(data.CsvContent!, targetCsvName, context);
        context.CancellationToken.ThrowIfCancellationRequested();

        var csvFilePath = Path.ChangeExtension(data.CsFilePath, ".csv");
        ResxFileSetWriter.Write(csvFilePath, records);
        context.CancellationToken.ThrowIfCancellationRequested();

        var sourceText = SourceTextBuilder.CreatePluginSource(data.Symbol, data.Modifiers, records);
        context.AddSource($"{data.Symbol.ContainingNamespace.ToDisplayString()}.{data.Symbol.Name}.g.cs", sourceText);
    }

    private static bool HasTargetAttribute(INamedTypeSymbol classSymbol, INamedTypeSymbol attributeSymbol) =>
        classSymbol.GetAttributes()
            .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

    private static IReadOnlyList<TranslateRecord> ReadCsvRecords(
        string csvContent,
        string fileName,
        SourceProductionContext context)
    {
        var config = CsvConfigurationFactory.Create(fileName, context.ReportDiagnostic);
        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<TranslateRecord>()
            .GroupBy(r => r.Key)
            .Select(g => g.First())
            .ToList();
    }
}