using System.Text.Json;
using System.Text.Json.Serialization;

namespace WordFlash.Core;

public sealed class SettingsStore
{
    private const long MaxFileBytes = 64 * 1024;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    public SettingsStore(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public static string DefaultPath =>
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WordFlash",
            "settings.json");

    /// <summary>
    /// Loads and normalizes the settings. A missing, oversized or unreadable file yields the defaults.
    /// </summary>
    public FlashSettings Load(Func<string, bool> fontExists)
    {
        FlashSettings? settings = null;
        try
        {
            var file = new FileInfo(Path);
            if (file.Exists && file.Length <= MaxFileBytes)
            {
                settings = JsonSerializer.Deserialize<FlashSettings>(File.ReadAllText(Path), s_jsonOptions);
            }
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            System.Diagnostics.Debug.WriteLine($"WordFlash: settings at {Path} unreadable, using defaults: {ex.Message}");
        }

        return (settings ?? new FlashSettings()).Normalize(fontExists);
    }

    /// <summary>
    /// Writes to a temporary file first so a crash mid-write cannot leave a truncated settings file.
    /// </summary>
    public void Save(FlashSettings settings)
    {
        var directory = System.IO.Path.GetDirectoryName(Path)
            ?? throw new InvalidOperationException($"Settings path {Path} has no directory.");
        Directory.CreateDirectory(directory);

        var tempPath = Path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, s_jsonOptions));
        File.Move(tempPath, Path, overwrite: true);
    }
}
