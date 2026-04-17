using Microsoft.CodeAnalysis;

namespace YukkuriMovieMaker.Generator;

internal static class DiagnosticDescriptors
{
    private const string Category = "YukkuriMovieMaker.Generator";

    internal static readonly DiagnosticDescriptor BadData = new(
        id: "YMMG001",
        title: "CSVデータの不正",
        messageFormat: "ファイル '{0}' の {1} 行目に不正なデータが見つかりました: {2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor FileNotFound = new(
        id: "YMMG002",
        title: "CSVファイルが見つかりません",
        messageFormat: "必要なCSVファイル '{0}' が見つかりませんでした",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}