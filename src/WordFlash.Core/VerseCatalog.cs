using System.Text.Json;
using System.Text.Json.Serialization;

namespace WordFlash.Core;

public sealed class VerseCatalog
{
    private const string ResourceName = "WordFlash.Core.verses.json";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    public VerseCatalog(IReadOnlyList<Verse> verses)
    {
        Verses = verses;
    }

    private Verse? _previous;

    public IReadOnlyList<Verse> Verses { get; }

    /// <summary>
    /// Picks a verse from the enabled categories, avoiding the one picked last time when there is a choice.
    /// Returns null when no verse is available.
    /// </summary>
    public Verse? PickRandom(IReadOnlySet<VerseCategory> enabled, Random rng)
    {
        var candidates = Verses.Where(v => enabled.Contains(v.Category) && v != _previous).ToList();
        if (candidates.Count == 0)
        {
            return _previous is not null && enabled.Contains(_previous.Category) ? _previous : null;
        }

        _previous = candidates[rng.Next(candidates.Count)];
        return _previous;
    }

    public static VerseCatalog LoadEmbedded()
    {
        using var stream = typeof(VerseCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing.");
        var verses = JsonSerializer.Deserialize<List<Verse>>(stream, JsonOptions)
            ?? throw new InvalidDataException($"Embedded resource {ResourceName} is empty.");
        return new VerseCatalog(verses);
    }
}
