using WordFlash.Core;
using WordFlash.DataGen;

namespace WordFlash.Tests;

public class VerseSelectorTests
{
    private static ParsedVerse Parsed(string book, string text, string? jesusText = null) =>
        new(book, 3, 5, text, jesusText);

    [Fact]
    public void Proverbs3_5_IsKept()
    {
        var verse = Parsed("PRO", "Trust in the LORD with all thine heart; and lean not unto thine own understanding.");

        var selected = Assert.Single(VerseSelector.Select([verse]));

        Assert.Equal(new Verse("Proverbs 3:5", VerseCategory.Proverbs, verse.Text), selected);
    }

    [Fact]
    public void Genealogy_IsExcluded()
    {
        var verse = Parsed("GEN", "And Seth lived an hundred and five years, and begat Enos.");

        Assert.Empty(VerseSelector.Select([verse]));
    }

    [Theory]
    [InlineData("The proverbs of Solomon the son of David, king of Israel;")]
    [InlineData("Jesus wept.")]
    public void FragmentOrTooShort_IsExcluded(string text) =>
        Assert.Empty(VerseSelector.Select([Parsed("PRO", text)]));

    [Fact]
    public void OverlongVerse_IsExcluded()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 70)) + ".";

        Assert.Empty(VerseSelector.Select([Parsed("PSA", text)]));
    }

    [Fact]
    public void GospelVerse_UsesOnlyJesusText()
    {
        var verse = Parsed(
            "MAT",
            "And Jesus answering said unto him, Suffer it to be so now: for thus it becometh us to fulfil all righteousness. Then he suffered him.",
            "Suffer it to be so now: for thus it becometh us to fulfil all righteousness.");

        var selected = Assert.Single(VerseSelector.Select([verse]));

        Assert.Equal(VerseCategory.JesusWords, selected.Category);
        Assert.Equal("Matthew 3:5", selected.Reference);
        Assert.Equal(verse.JesusText, selected.Text);
    }

    [Fact]
    public void GospelNarrationWithoutJesusText_IsExcluded() =>
        Assert.Empty(VerseSelector.Select([Parsed("LUK", "And they came with haste, and found Mary, and Joseph, and the babe lying in a manger.")]));

    [Theory]
    [InlineData("GEN", VerseCategory.Torah, "Genesis")]
    [InlineData("DEU", VerseCategory.Torah, "Deuteronomy")]
    [InlineData("PSA", VerseCategory.Psalms, "Psalm")]
    [InlineData("ISA", VerseCategory.Prophets, "Isaiah")]
    [InlineData("MAL", VerseCategory.Prophets, "Malachi")]
    [InlineData("ROM", VerseCategory.PaulsEpistles, "Romans")]
    [InlineData("1CO", VerseCategory.PaulsEpistles, "1 Corinthians")]
    [InlineData("HEB", VerseCategory.PaulsEpistles, "Hebrews")]
    public void Book_MapsToCategoryAndName(string book, VerseCategory category, string name)
    {
        var selected = Assert.Single(VerseSelector.Select([Parsed(book, "For the LORD is good; his mercy is everlasting.")]));

        Assert.Equal(category, selected.Category);
        Assert.Equal($"{name} 3:5", selected.Reference);
    }

    [Theory]
    [InlineData("JOS")]
    [InlineData("ACT")]
    [InlineData("JAS")]
    [InlineData("REV")]
    public void BookOutsideCategories_IsExcluded(string book) =>
        Assert.Empty(VerseSelector.Select([Parsed(book, "For the LORD is good; his mercy is everlasting.")]));
}
