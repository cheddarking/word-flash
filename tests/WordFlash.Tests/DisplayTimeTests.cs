using WordFlash.Core;

namespace WordFlash.Tests;

public class DisplayTimeTests
{
    private static Verse VerseOfWords(int words) =>
        new("Psalm 1:1", VerseCategory.Psalms, string.Join(' ', Enumerable.Repeat("word", words)) + ".");

    [Fact]
    public void LongerVerse_StaysLonger()
    {
        var shortTime = DisplayTime.For(VerseOfWords(10), ReadingSpeed.Normal, new Random(5));
        var longTime = DisplayTime.For(VerseOfWords(50), ReadingSpeed.Normal, new Random(5));

        Assert.True(longTime > shortTime, $"{longTime} should exceed {shortTime}");
    }

    [Theory]
    [InlineData(23, ReadingSpeed.Normal)]
    [InlineData(40, ReadingSpeed.Slow)]
    [InlineData(60, ReadingSpeed.Fast)]
    public void WithinReadingTimeAndTwentyPercentExtra(int textWords, ReadingSpeed speed)
    {
        var verse = VerseOfWords(textWords);
        var words = textWords + 2;
        var readingSeconds = 2 + (words * 60.0 / (int)speed);
        var rng = new Random(9);

        for (var i = 0; i < 300; i++)
        {
            var seconds = DisplayTime.For(verse, speed, rng).TotalSeconds;
            Assert.InRange(seconds, readingSeconds - 0.05, (readingSeconds * 1.2) + 0.05);
        }
    }

    [Fact]
    public void Normal_TypicalVerseOf25Words_TakesAboutTenToTwelveSeconds()
    {
        var seconds = DisplayTime.For(VerseOfWords(23), ReadingSpeed.Normal, new Random(1)).TotalSeconds;

        Assert.InRange(seconds, 10.3 - 0.05, 12.4 + 0.05);
    }

    [Fact]
    public void ClampedToFourAndSixtySeconds()
    {
        var tiny = new Verse("A 1:1", VerseCategory.Psalms, "Yes.");

        Assert.Equal(TimeSpan.FromSeconds(4), DisplayTime.For(tiny, ReadingSpeed.Fast, new Random(1)));
        Assert.Equal(TimeSpan.FromSeconds(60), DisplayTime.For(VerseOfWords(500), ReadingSpeed.Slow, new Random(1)));
    }

    [Fact]
    public void SlowerSpeed_StaysLonger()
    {
        var verse = VerseOfWords(30);

        var slow = DisplayTime.For(verse, ReadingSpeed.Slow, new Random(3));
        var normal = DisplayTime.For(verse, ReadingSpeed.Normal, new Random(3));
        var fast = DisplayTime.For(verse, ReadingSpeed.Fast, new Random(3));

        Assert.True(slow > normal && normal > fast, $"{slow} > {normal} > {fast}");
    }

    [Fact]
    public void ExtraIsRandom_NotAlwaysTheSame()
    {
        var verse = VerseOfWords(30);
        var rng = new Random(4);

        var distinct = Enumerable.Range(0, 20).Select(_ => DisplayTime.For(verse, ReadingSpeed.Normal, rng)).Distinct().Count();

        Assert.True(distinct > 5);
    }
}
