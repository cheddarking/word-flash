using System.Text;
using System.Text.RegularExpressions;

namespace WordFlash.DataGen;

/// <summary>
/// Reads the subset of USFM used by the eBible.org KJV (eng-kjv2006) edition.
/// </summary>
public static partial class UsfmParser
{
    private const string JesusOpen = "\u0001";
    private const string JesusClose = "\u0002";

    public static IReadOnlyList<ParsedVerse> ParseBook(string usfm)
    {
        var book = IdLine().Match(usfm) is { Success: true } id
            ? id.Groups[1].Value
            : throw new FormatException("USFM book has no \\id line.");

        var body = Footnote().Replace(usfm, string.Empty);
        body = WordWithAttributes().Replace(body, "$1");
        body = CharacterStyle().Replace(body, string.Empty);
        body = body.Replace("\\wj*", JesusClose, StringComparison.Ordinal)
                   .Replace("\\wj ", JesusOpen, StringComparison.Ordinal);

        var verses = new List<ParsedVerse>();
        var chapter = 0;
        var inJesusWords = false;
        int? verseNumber = null;
        var text = new StringBuilder();
        var jesus = new StringBuilder();

        void Flush()
        {
            if (verseNumber is null)
            {
                return;
            }

            var jesusText = Clean(jesus.ToString());
            verses.Add(new ParsedVerse(
                book,
                chapter,
                verseNumber.Value,
                Clean(text.ToString()),
                jesusText.Length == 0 ? null : jesusText));
            verseNumber = null;
            text.Clear();
            jesus.Clear();
        }

        foreach (var rawLine in body.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || SkippedLine().IsMatch(line))
            {
                continue;
            }

            if (Chapter().Match(line) is { Success: true } c)
            {
                Flush();
                chapter = int.Parse(c.Groups[1].Value);
                continue;
            }

            if (Verse().Match(line) is { Success: true } v)
            {
                Flush();
                verseNumber = int.Parse(v.Groups[1].Value);
                line = line[v.Length..];
            }

            if (verseNumber is null)
            {
                continue;
            }

            foreach (var ch in line)
            {
                if (ch == JesusOpen[0])
                {
                    inJesusWords = true;
                    continue;
                }

                if (ch == JesusClose[0])
                {
                    inJesusWords = false;
                    continue;
                }

                text.Append(ch);
                if (inJesusWords)
                {
                    jesus.Append(ch);
                }
            }

            text.Append(' ');
            if (inJesusWords)
            {
                jesus.Append(' ');
            }
        }

        Flush();
        return verses;
    }

    private static string Clean(string value)
    {
        var withoutMarkers = RemainingMarker().Replace(value, " ").Replace("¶", " ", StringComparison.Ordinal);
        return Whitespace().Replace(withoutMarkers, " ").Trim();
    }

    [GeneratedRegex(@"^\\id (\w+)", RegexOptions.Multiline)]
    private static partial Regex IdLine();

    [GeneratedRegex(@"\\f .*?\\f\*", RegexOptions.Singleline)]
    private static partial Regex Footnote();

    [GeneratedRegex(@"\\\+?w ([^|\\]*)(?:\|[^\\]*)?\\\+?w\*")]
    private static partial Regex WordWithAttributes();

    [GeneratedRegex(@"\\\+?(?:add|nd|tl)\*|\\\+?(?:add|nd|tl) ")]
    private static partial Regex CharacterStyle();

    [GeneratedRegex(@"^\\(?:id|h|toc\d|mt\d?|s\d?|d|r|ms\d?)\b")]
    private static partial Regex SkippedLine();

    [GeneratedRegex(@"^\\c (\d+)")]
    private static partial Regex Chapter();

    [GeneratedRegex(@"^\\v (\d+)\s*")]
    private static partial Regex Verse();

    [GeneratedRegex(@"\\[a-z]+\d?\*?")]
    private static partial Regex RemainingMarker();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
