using System.Text.RegularExpressions;
using WordFlash.Core;

namespace WordFlash.DataGen;

/// <summary>
/// Chooses which parsed verses can stand alone on screen, and files them under a category.
/// </summary>
public static partial class VerseSelector
{
    public const int MinWords = 6;
    public const int MaxCharacters = 280;

    private static readonly Dictionary<string, (string Name, VerseCategory Category)> s_books = new()
    {
        ["GEN"] = ("Genesis", VerseCategory.Torah),
        ["EXO"] = ("Exodus", VerseCategory.Torah),
        ["LEV"] = ("Leviticus", VerseCategory.Torah),
        ["NUM"] = ("Numbers", VerseCategory.Torah),
        ["DEU"] = ("Deuteronomy", VerseCategory.Torah),
        ["PSA"] = ("Psalm", VerseCategory.Psalms),
        ["PRO"] = ("Proverbs", VerseCategory.Proverbs),
        ["ISA"] = ("Isaiah", VerseCategory.Prophets),
        ["JER"] = ("Jeremiah", VerseCategory.Prophets),
        ["LAM"] = ("Lamentations", VerseCategory.Prophets),
        ["EZK"] = ("Ezekiel", VerseCategory.Prophets),
        ["DAN"] = ("Daniel", VerseCategory.Prophets),
        ["HOS"] = ("Hosea", VerseCategory.Prophets),
        ["JOL"] = ("Joel", VerseCategory.Prophets),
        ["AMO"] = ("Amos", VerseCategory.Prophets),
        ["OBA"] = ("Obadiah", VerseCategory.Prophets),
        ["JON"] = ("Jonah", VerseCategory.Prophets),
        ["MIC"] = ("Micah", VerseCategory.Prophets),
        ["NAM"] = ("Nahum", VerseCategory.Prophets),
        ["HAB"] = ("Habakkuk", VerseCategory.Prophets),
        ["ZEP"] = ("Zephaniah", VerseCategory.Prophets),
        ["HAG"] = ("Haggai", VerseCategory.Prophets),
        ["ZEC"] = ("Zechariah", VerseCategory.Prophets),
        ["MAL"] = ("Malachi", VerseCategory.Prophets),
        ["MAT"] = ("Matthew", VerseCategory.JesusWords),
        ["MRK"] = ("Mark", VerseCategory.JesusWords),
        ["LUK"] = ("Luke", VerseCategory.JesusWords),
        ["JHN"] = ("John", VerseCategory.JesusWords),
        ["ROM"] = ("Romans", VerseCategory.PaulsEpistles),
        ["1CO"] = ("1 Corinthians", VerseCategory.PaulsEpistles),
        ["2CO"] = ("2 Corinthians", VerseCategory.PaulsEpistles),
        ["GAL"] = ("Galatians", VerseCategory.PaulsEpistles),
        ["EPH"] = ("Ephesians", VerseCategory.PaulsEpistles),
        ["PHP"] = ("Philippians", VerseCategory.PaulsEpistles),
        ["COL"] = ("Colossians", VerseCategory.PaulsEpistles),
        ["1TH"] = ("1 Thessalonians", VerseCategory.PaulsEpistles),
        ["2TH"] = ("2 Thessalonians", VerseCategory.PaulsEpistles),
        ["1TI"] = ("1 Timothy", VerseCategory.PaulsEpistles),
        ["2TI"] = ("2 Timothy", VerseCategory.PaulsEpistles),
        ["TIT"] = ("Titus", VerseCategory.PaulsEpistles),
        ["PHM"] = ("Philemon", VerseCategory.PaulsEpistles),
        ["HEB"] = ("Hebrews", VerseCategory.PaulsEpistles),
    };

    public static IEnumerable<string> BookCodes => s_books.Keys;

    public static IEnumerable<Verse> Select(IEnumerable<ParsedVerse> verses)
    {
        foreach (var parsed in verses)
        {
            if (!s_books.TryGetValue(parsed.Book, out var book))
            {
                continue;
            }

            var text = book.Category == VerseCategory.JesusWords ? parsed.JesusText : parsed.Text;
            if (text is null || !IsStandalone(text))
            {
                continue;
            }

            yield return new Verse($"{book.Name} {parsed.Chapter}:{parsed.Verse}", book.Category, text);
        }
    }

    public static bool IsStandalone(string text) =>
        text.Length <= MaxCharacters
        && text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= MinWords
        && text[^1] is '.' or '?' or '!'
        && !Genealogy().IsMatch(text);

    [GeneratedRegex(@"\bbegat\b", RegexOptions.IgnoreCase)]
    private static partial Regex Genealogy();
}
