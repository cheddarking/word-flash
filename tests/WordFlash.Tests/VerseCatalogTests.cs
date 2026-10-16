using WordFlash.Core;

namespace WordFlash.Tests;

public class VerseCatalogTests
{
    private readonly VerseCatalog _catalog = new(
    [
        new Verse("Proverbs 1:1", VerseCategory.Proverbs, "a"),
        new Verse("Proverbs 1:2", VerseCategory.Proverbs, "b"),
        new Verse("Psalm 1:1", VerseCategory.Psalms, "c"),
        new Verse("Matthew 5:3", VerseCategory.JesusWords, "d"),
    ]);

    [Fact]
    public void PickRandom_OnlyReturnsEnabledCategories()
    {
        var rng = new Random(42);
        var enabled = new HashSet<VerseCategory> { VerseCategory.Proverbs };

        for (var i = 0; i < 50; i++)
        {
            Assert.Equal(VerseCategory.Proverbs, _catalog.PickRandom(enabled, rng)!.Category);
        }
    }

    [Fact]
    public void PickRandom_NeverRepeatsPreviousVerse()
    {
        var rng = new Random(7);
        var enabled = new HashSet<VerseCategory> { VerseCategory.Proverbs };
        var previous = _catalog.PickRandom(enabled, rng);

        for (var i = 0; i < 50; i++)
        {
            var next = _catalog.PickRandom(enabled, rng);
            Assert.NotEqual(previous, next);
            previous = next;
        }
    }

    [Fact]
    public void PickRandom_SingleCandidate_IsReturnedEveryTime()
    {
        var rng = new Random(1);
        var enabled = new HashSet<VerseCategory> { VerseCategory.Psalms };

        Assert.Equal("Psalm 1:1", _catalog.PickRandom(enabled, rng)!.Reference);
        Assert.Equal("Psalm 1:1", _catalog.PickRandom(enabled, rng)!.Reference);
    }

    [Fact]
    public void PickRandom_NothingEnabled_ReturnsNull() =>
        Assert.Null(_catalog.PickRandom(new HashSet<VerseCategory>(), new Random(1)));

    [Fact]
    public void PickRandom_EnabledCategoryWithNoVerses_ReturnsNull() =>
        Assert.Null(_catalog.PickRandom(new HashSet<VerseCategory> { VerseCategory.Torah }, new Random(1)));

    [Fact]
    public void PickRandom_CoversAllEnabledVerses()
    {
        var rng = new Random(3);
        var enabled = Enum.GetValues<VerseCategory>().ToHashSet();
        var seen = new HashSet<string>();

        for (var i = 0; i < 200; i++)
        {
            seen.Add(_catalog.PickRandom(enabled, rng)!.Reference);
        }

        Assert.Equal(4, seen.Count);
    }
}
