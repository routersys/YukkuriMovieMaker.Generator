using System.Collections.Generic;
using System.IO;

namespace YukkuriMovieMaker.Generator;

internal static class ResxFileSetWriter
{
    internal static void Write(string csvFilePath, IReadOnlyList<TranslateRecord> records)
    {
        foreach (var langCode in TranslateRecord.LangCodes)
        {
            var resxPath = BuildResxPath(csvFilePath, langCode);
            using var writer = new ResXResourceWriter(resxPath);
            writer.AddResource("CurrentCulture", langCode);
            WriteRecords(writer, records, langCode);
        }
    }

    private static string BuildResxPath(string csvFilePath, string langCode) =>
        langCode == TranslateRecord.PrimaryLangCode
            ? Path.ChangeExtension(csvFilePath, ".resx")
            : Path.ChangeExtension(csvFilePath, $".{langCode}.resx");

    private static void WriteRecords(ResXResourceWriter writer, IReadOnlyList<TranslateRecord> records, string langCode)
    {
        foreach (var record in records)
        {
            var value = record.GetValue(langCode);
            if (!string.IsNullOrEmpty(record.Key) && !string.IsNullOrEmpty(value))
                writer.AddResource(record.Key!, value!);
        }
    }
}