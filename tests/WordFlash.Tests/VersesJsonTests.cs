using WordFlash.Core;

namespace WordFlash.Tests;

public class VersesJsonTests
{
    private static readonly IReadOnlyList<Verse> s_verses = VerseCatalog.LoadEmbedded().Verses;

    [Fact]
    public void AllCategoriesPresent_WithAtLeast300Each()
    {
        foreach (var category in Enum.GetValues<VerseCategory>())
        {
            Assert.True(
                s_verses.Count(v => v.Category == category) >= 300,
                $"{category} has fewer than 300 verses.");
        }
    }

    [Fact]
    public void NoMarkupRemains()
    {
        Assert.DoesNotContain(s_verses, v => v.Text.Contains('\\') || v.Text.Contains("strong=") || v.Text.Contains('¶'));
    }

    [Fact]
    public void Matthew3_15_IsOnlyTheWordsOfJesus()
    {
        var verse = Assert.Single(s_verses, v => v.Reference == "Matthew 3:15");

        Assert.Equal("Suffer it to be so now: for thus it becometh us to fulfil all righteousness.", verse.Text);
        Assert.Equal(VerseCategory.JesusWords, verse.Category);
    }

    [Fact]
    public void Proverbs3_5_IsPresent()
    {
        var verse = Assert.Single(s_verses, v => v.Reference == "Proverbs 3:5");

        Assert.Equal("Trust in the LORD with all thine heart; and lean not unto thine own understanding.", verse.Text);
    }

    [Fact]
    public void ReferencesAreUnique() =>
        Assert.Equal(s_verses.Count, s_verses.Select(v => v.Reference).Distinct().Count());
}
