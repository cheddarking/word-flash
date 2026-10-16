using WordFlash.DataGen;

namespace WordFlash.Tests;

public class UsfmParserTests
{
    private const string Matthew3 = """
        \id MAT  The Gospel According to Matthew
        \c 3
        \p
        \v 14 \w But|strong="G1161"\w* \w John|strong="G2491"\w* \w forbad|strong="G1254"\w* \w him|strong="G0846"\w*.
        \v 15 \w And|strong="G1161"\w* \w Jesus|strong="G2424"\w* \w answering|strong="G0611"\w* \w said|strong="G2036"\w* \w unto|strong="G4314"\w* \w him|strong="G0846"\w*, \wj  \+w Suffer|strong="G0863"\+w* \+add  \+w it to be so|strong="G2076"\+w*\+add* \+w now|strong="G0737"\+w*: \+w for|strong="G1063"\+w* \+w thus|strong="G3779"\+w* \+w it becometh|strong="G4241"\+w* \+w us|strong="G2254"\+w* \+w to fulfil|strong="G4137"\+w* \+w all|strong="G3956"\+w* \+w righteousness|strong="G1343"\+w*.\wj* \w Then|strong="G5119"\w* he \w suffered|strong="G0863"\w* \w him|strong="G0846"\w*.
        """;

    [Fact]
    public void Mat3_15_ReturnsJesusSpanOnly()
    {
        var verses = UsfmParser.ParseBook(Matthew3);

        var verse = Assert.Single(verses, v => v.Verse == 15);
        Assert.Equal("Suffer it to be so now: for thus it becometh us to fulfil all righteousness.", verse.JesusText);
    }

    [Fact]
    public void Mat3_15_FullTextHasNoMarkup()
    {
        var verse = Assert.Single(UsfmParser.ParseBook(Matthew3), v => v.Verse == 15);

        Assert.Equal(
            "And Jesus answering said unto him, Suffer it to be so now: for thus it becometh us to fulfil all righteousness. Then he suffered him.",
            verse.Text);
        Assert.Equal("MAT", verse.Book);
        Assert.Equal(3, verse.Chapter);
    }

    [Fact]
    public void VerseWithoutWordsOfJesus_HasNullJesusText()
    {
        var verse = Assert.Single(UsfmParser.ParseBook(Matthew3), v => v.Verse == 14);

        Assert.Null(verse.JesusText);
        Assert.Equal("But John forbad him.", verse.Text);
    }

    [Fact]
    public void Pro3_5_KeepsDivineNameAndDropsPilcrow()
    {
        const string usfm = """
            \id PRO
            \c 3
            \v 5 ¶ \w Trust|strong="H0982"\w* in the \nd \+w LORD|strong="H3068"\+w*\nd* with all thine \w heart|strong="H3820"\w*; and \w lean|strong="H8172"\w* not unto thine own \w understanding|strong="H0998"\w*.
            """;

        var verse = Assert.Single(UsfmParser.ParseBook(usfm));

        Assert.Equal("Trust in the LORD with all thine heart; and lean not unto thine own understanding.", verse.Text);
    }

    [Fact]
    public void Footnote_IsRemoved_AndItalicAddKeepsWord()
    {
        const string usfm = """
            \id PRO
            \c 1
            \v 3 To \w receive|strong="H3947"\w* \add the\add* \w instruction|strong="H4148"\w* of \w wisdom|strong="H7919"\w*, and \w equity|strong="H4339"\w*;\f + \fr 1.3 \ft equity: Heb. equities\f*
            """;

        var verse = Assert.Single(UsfmParser.ParseBook(usfm));

        Assert.Equal("To receive the instruction of wisdom, and equity;", verse.Text);
    }

    [Fact]
    public void PsalmTitleAndSectionHeading_AreNotPartOfVerse()
    {
        const string usfm = """
            \id PSA
            \c 3
            \d A \w Psalm|strong="H4210"\w* of \w David|strong="H1732"\w*, when he fled from Absalom his son.
            \s1 A heading
            \q1
            \v 1 \nd \+w LORD|strong="H3068"\+w*\nd*, how are they increased that trouble me!
            """;

        var verse = Assert.Single(UsfmParser.ParseBook(usfm));

        Assert.Equal("LORD, how are they increased that trouble me!", verse.Text);
    }

    [Fact]
    public void WordsOfJesusSpanningTwoVerses_AreSplitPerVerse()
    {
        const string usfm = """
            \id JHN
            \c 1
            \v 1 He said, \wj Come
            \v 2 and see.\wj* So they came.
            """;

        var verses = UsfmParser.ParseBook(usfm);

        Assert.Equal("Come", verses[0].JesusText);
        Assert.Equal("and see.", verses[1].JesusText);
    }
}
