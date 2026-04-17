using CsvHelper.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;

namespace YukkuriMovieMaker.Generator;

internal static class CsvConfigurationFactory
{
    internal static CsvConfiguration Create(string filePath, Action<Diagnostic> reportDiagnostic) =>
        new(System.Globalization.CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            BadDataFound = args =>
            {
                var lineIndex = Math.Max(0, args.Context.Parser?.Row - 1 ?? 0);
                var rawRecord = args.RawRecord?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
                reportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.BadData,
                    Location.Create(
                        filePath,
                        new TextSpan(0, 0),
                        new LinePositionSpan(
                            new LinePosition(lineIndex, 0),
                            new LinePosition(lineIndex, 0))),
                    Path.GetFileName(filePath),
                    args.Context.Parser?.Row ?? 0,
                    rawRecord));
            },
        };
}