using CsvHelper.Configuration.Attributes;

namespace YukkuriMovieMaker.Generator;

internal sealed class TranslateRecord
{
    private const string JaJp = "ja-jp";
    private const string EnUs = "en-us";
    private const string ZhCn = "zh-cn";
    private const string ZhTw = "zh-tw";
    private const string KoKr = "ko-kr";
    private const string EsEs = "es-es";
    private const string ArSa = "ar-sa";
    private const string IdId = "id-id";

    internal const string PrimaryLangCode = JaJp;

    internal static readonly string[] LangCodes =
    [
        JaJp, EnUs, ZhCn, ZhTw, KoKr, EsEs, ArSa, IdId,
    ];

    [Index(0)] public string? Key { get; set; }
    [Index(1)] public string? Comment { get; set; }
    [Index(2)] public string? Japanese { get; set; }
    [Index(3)] public string? English { get; set; }
    [Index(4)] public string? SimplifiedChinese { get; set; }
    [Index(5)] public string? TraditionalChinese { get; set; }
    [Index(6)] public string? Korean { get; set; }
    [Index(7)] public string? Spanish { get; set; }
    [Index(8)] public string? Arabic { get; set; }
    [Index(9)] public string? BahasaIndonesia { get; set; }
    [Index(10)] public string? Unknown { get; set; }

    internal string? GetValue(string langCode) => langCode switch
    {
        JaJp => Japanese,
        EnUs => English,
        ZhCn => SimplifiedChinese,
        ZhTw => TraditionalChinese,
        KoKr => Korean,
        EsEs => Spanish,
        ArSa => Arabic,
        IdId => BahasaIndonesia,
        _ => Unknown,
    };
}