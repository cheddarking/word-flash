using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WordFlash.Core;
using WordFlash.DataGen;

// Pinned to the archive audited for this release; see data/SOURCE.md.
const string ExpectedSha256 = "5789dcd000d60a554abb4cf9b37ce10a53628bfc686e11ada2d6c981455239c1";

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: WordFlash.DataGen <eng-kjv2006_usfm.zip> <verses.json>");
    return 2;
}

var (zipPath, outputPath) = (args[0], args[1]);

var actualSha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(zipPath)));
if (actualSha256 != ExpectedSha256)
{
    Console.Error.WriteLine($"SHA-256 mismatch for {zipPath}: expected {ExpectedSha256}, got {actualSha256}.");
    return 1;
}

var wanted = VerseSelector.BookCodes.ToHashSet();
var parsed = new List<ParsedVerse>();
using (var zip = ZipFile.OpenRead(zipPath))
{
    foreach (var entry in zip.Entries.Where(e => e.Name.EndsWith(".usfm", StringComparison.OrdinalIgnoreCase)))
    {
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        var book = UsfmParser.ParseBook(reader.ReadToEnd());
        if (book.Count > 0 && wanted.Contains(book[0].Book))
        {
            parsed.AddRange(book);
        }
    }
}

var verses = VerseSelector.Select(parsed).ToList();

// One verse per line keeps the committed file reviewable and diff-friendly.
var json = new StringBuilder("[\n");
json.AppendJoin(",\n", verses.Select(v => "  " + JsonSerializer.Serialize(v, VerseCatalog.JsonOptions)));
json.Append("\n]\n");
File.WriteAllText(outputPath, json.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

foreach (var group in verses.GroupBy(v => v.Category))
{
    Console.WriteLine($"{group.Key}: {group.Count()}");
}

Console.WriteLine($"Wrote {verses.Count} verses to {outputPath}");
return 0;
