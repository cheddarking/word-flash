using WordFlash.Core;

namespace WordFlash.Tests;

public class FlashSchedulerTests
{
    private static readonly VerseCatalog s_catalog = new(
    [
        new Verse("Proverbs 3:5", VerseCategory.Proverbs, "Trust in the LORD with all thine heart."),
        new Verse("Psalm 23:1", VerseCategory.Psalms, "The LORD is my shepherd; I shall not want."),
    ]);

    [Fact]
    public void Delay_WithinConfiguredGap()
    {
        var settings = new FlashSettings { GapMinMinutes = 1, GapMaxMinutes = 2 };
        var rng = new Random(11);

        for (var i = 0; i < 500; i++)
        {
            var plan = FlashScheduler.NextFlash(settings, s_catalog, rng)!;
            Assert.InRange(plan.Delay, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));
        }
    }

    [Fact]
    public void Duration_MatchesDisplayTimeForTheChosenVerse()
    {
        var settings = new FlashSettings { ReadingSpeed = ReadingSpeed.Slow };

        var plan = FlashScheduler.NextFlash(settings, s_catalog, new Random(12))!;

        var words = (plan.Verse.Text + " " + plan.Verse.Reference).Split(' ').Length;
        var reading = Math.Max(4, 2 + (words * 60.0 / 150));
        Assert.InRange(plan.Duration.TotalSeconds, reading - 0.05, (reading * 1.2) + 0.05);
    }

    [Fact]
    public void EqualGapMinAndMax_GivesExactDelay()
    {
        var settings = new FlashSettings { GapMinMinutes = 3, GapMaxMinutes = 3 };

        var plan = FlashScheduler.NextFlash(settings, s_catalog, new Random(1))!;

        Assert.Equal(TimeSpan.FromMinutes(3), plan.Delay);
    }

    [Fact]
    public void EveryPosition_IsReachable()
    {
        var rng = new Random(13);
        var seen = new HashSet<OverlayPosition>();

        for (var i = 0; i < 200; i++)
        {
            seen.Add(FlashScheduler.NextFlash(new FlashSettings(), s_catalog, rng)!.Position);
        }

        Assert.Equal(Enum.GetValues<OverlayPosition>().ToHashSet(), seen);
    }

    [Fact]
    public void Verse_ComesFromEnabledCategories()
    {
        var settings = new FlashSettings { Categories = [VerseCategory.Psalms] };

        var plan = FlashScheduler.NextFlash(settings, s_catalog, new Random(1))!;

        Assert.Equal("Psalm 23:1", plan.Verse.Reference);
    }

    [Fact]
    public void NoCategoriesEnabled_ReturnsNull() =>
        Assert.Null(FlashScheduler.NextFlash(new FlashSettings { Categories = [] }, s_catalog, new Random(1)));
}
