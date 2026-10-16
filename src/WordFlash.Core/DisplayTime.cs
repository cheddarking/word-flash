namespace WordFlash.Core;

/// <summary>
/// How long a verse stays on screen: time to notice it, plus time to read it, plus a little random slack.
/// </summary>
public static class DisplayTime
{
    public const double NoticeSeconds = 2;
    public const double MaxExtraFraction = 0.2;
    public const int MinSeconds = 4;
    public const int MaxSeconds = 60;

    public static TimeSpan For(Verse verse, ReadingSpeed speed, Random rng)
    {
        var words = CountWords(verse.Text) + CountWords(verse.Reference);
        var readingSeconds = NoticeSeconds + (words * 60.0 / (int)speed);
        var seconds = readingSeconds * (1 + (rng.NextDouble() * MaxExtraFraction));
        return TimeSpan.FromSeconds(Math.Round(Math.Clamp(seconds, MinSeconds, MaxSeconds), 1));
    }

    private static int CountWords(string text) => text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}
