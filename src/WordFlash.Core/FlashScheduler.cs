namespace WordFlash.Core;

public static class FlashScheduler
{
    /// <summary>
    /// Draws the next flash: the gap before it, where it appears, the verse, and how long it stays
    /// (long enough to read that verse).
    /// Returns null when no enabled category has a verse.
    /// </summary>
    public static FlashPlan? NextFlash(FlashSettings settings, VerseCatalog catalog, Random rng)
    {
        var verse = catalog.PickRandom(settings.Categories.ToHashSet(), rng);
        if (verse is null)
        {
            return null;
        }

        var delaySeconds = Between(rng, settings.GapMinMinutes * 60, settings.GapMaxMinutes * 60);
        var positions = Enum.GetValues<OverlayPosition>();

        return new FlashPlan(
            TimeSpan.FromSeconds(delaySeconds),
            positions[rng.Next(positions.Length)],
            DisplayTime.For(verse, settings.ReadingSpeed, rng),
            verse);
    }

    private static int Between(Random rng, int a, int b) => rng.Next(Math.Min(a, b), Math.Max(a, b) + 1);
}
