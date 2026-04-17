using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YukkuriMovieMaker.Generator;

internal static class SourceTextBuilder
{
    private const string AttributeSource = """
        namespace YukkuriMovieMaker.Generator
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            sealed class AutoGenLocalizerAttribute : System.Attribute
            {
            }
        }
        """;

    internal static SourceText CreateAttributeSource() =>
        SourceText.From(AttributeSource, Encoding.UTF8);

    internal static SourceText CreatePluginSource(
        INamedTypeSymbol symbol,
        SyntaxTokenList modifiers,
        IReadOnlyList<TranslateRecord> records)
    {
        var namespaceName = symbol.ContainingNamespace.ToDisplayString();
        var className = symbol.Name;
        var modifierString = string.Join(" ", modifiers.Select(m => m.ToString()));
        var propertyDeclarations = BuildPropertyDeclarations(records);

        var source = $$"""
using YukkuriMovieMaker.Plugin;

namespace {{namespaceName}}
{
    {{modifierString}} class {{className}} : ILocalizePlugin
    {
        public string Name => "{{className}}多言語対応プラグイン（自動生成）";
        public void SetCulture(System.Globalization.CultureInfo cultureInfo) => {{className}}.Culture = cultureInfo;

        private static System.Resources.ResourceManager? _resourceManager;
        private static System.Globalization.CultureInfo? _cultureInfo;

        public static System.Resources.ResourceManager ResourceManager =>
            _resourceManager ??= new System.Resources.ResourceManager(typeof({{className}}));

        public static System.Globalization.CultureInfo? Culture
        {
            get => _cultureInfo;
            set => _cultureInfo = value;
        }

        public static string GetString(string key) =>
            ResourceManager.GetString(key, _cultureInfo) ?? key;

        public static string CurrentCulture => GetString("CurrentCulture");
{{propertyDeclarations}}
    }
}
""";

        return SourceText.From(source, Encoding.UTF8);
    }

    private static string BuildPropertyDeclarations(IReadOnlyList<TranslateRecord> records) =>
        string.Join(
            "\n",
            records
                .Where(r => !string.IsNullOrEmpty(r.Key))
                .Select(r => $"        public static string {r.Key} => GetString(\"{r.Key}\");"));
}