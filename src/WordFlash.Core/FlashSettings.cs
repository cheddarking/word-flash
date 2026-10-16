namespace WordFlash.Core;

public sealed record FlashSettings
{
    public const string DefaultFontFamily = "Segoe UI";
    public const int MinGapMinutes = 1;
    public const int MaxGapMinutes = 240;
    public const int MinFontSize = 12;
    public const int MaxFontSize = 96;

    public bool Enabled { get; init; } = true;

    public IReadOnlyList<VerseCategory> Categories { get; init; } = Enum.GetValues<VerseCategory>();

    public ReadingSpeed ReadingSpeed { get; init; } = ReadingSpeed.Normal;

    public int GapMinMinutes { get; init; } = 5;

    public int GapMaxMinutes { get; init; } = 15;

    public string FontFamily { get; init; } = DefaultFontFamily;

    public int FontSize { get; init; } = 28;

    public bool StartWithWindows { get; init; }

    /// <summary>
    /// Returns a copy with every value inside its limits, ranges ordered min to max,
    /// and a font that <paramref name="fontExists"/> accepts.
    /// </summary>
    public FlashSettings Normalize(Func<string, bool> fontExists)
    {
        var (gapMin, gapMax) = OrderedRange(GapMinMinutes, GapMaxMinutes, MinGapMinutes, MaxGapMinutes);

        return this with
        {
            Categories = (Categories ?? []).Where(Enum.IsDefined).Distinct().ToArray(),
            ReadingSpeed = Enum.IsDefined(ReadingSpeed) ? ReadingSpeed : ReadingSpeed.Normal,
            GapMinMinutes = gapMin,
            GapMaxMinutes = gapMax,
            FontFamily = !string.IsNullOrWhiteSpace(FontFamily) && fontExists(FontFamily) ? FontFamily : DefaultFontFamily,
            FontSize = Math.Clamp(FontSize, MinFontSize, MaxFontSize),
        };
    }

    private static (int Min, int Max) OrderedRange(int a, int b, int lowest, int highest)
    {
        var low = Math.Clamp(Math.Min(a, b), lowest, highest);
        var high = Math.Clamp(Math.Max(a, b), lowest, highest);
        return (low, high);
    }
}
