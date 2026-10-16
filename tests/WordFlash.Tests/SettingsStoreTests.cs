using WordFlash.Core;

namespace WordFlash.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private static readonly Func<string, bool> s_anyFont = _ => true;

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "WordFlashTests", Guid.NewGuid().ToString("N"));
    private readonly string _path;
    private readonly SettingsStore _store;

    public SettingsStoreTests()
    {
        _path = Path.Combine(_directory, "settings.json");
        _store = new SettingsStore(_path);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private void WriteRaw(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(_path, json);
    }

    [Fact]
    public void MissingFile_LoadsDefaults()
    {
        var settings = _store.Load(s_anyFont);

        Assert.True(settings.Enabled);
        Assert.Equal(Enum.GetValues<VerseCategory>(), settings.Categories.Order());
        Assert.Equal(ReadingSpeed.Normal, settings.ReadingSpeed);
        Assert.Equal((5, 15), (settings.GapMinMinutes, settings.GapMaxMinutes));
        Assert.Equal("Segoe UI", settings.FontFamily);
        Assert.Equal(28, settings.FontSize);
        Assert.False(settings.StartWithWindows);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("""{ "gapMinMinutes": 99999999999 }""")]
    [InlineData("""{ "categories": ["NotACategory"] }""")]
    public void CorruptFile_LoadsDefaults(string json)
    {
        WriteRaw(json);

        var settings = _store.Load(s_anyFont);

        Assert.Equal(new FlashSettings().Normalize(s_anyFont).GapMinMinutes, settings.GapMinMinutes);
        Assert.Equal(6, settings.Categories.Count);
    }

    [Fact]
    public void RoundTrip_PreservesGapRange()
    {
        var saved = new FlashSettings { GapMinMinutes = 1, GapMaxMinutes = 2 };

        _store.Save(saved);
        var loaded = _store.Load(s_anyFont);

        Assert.Equal((1, 2), (loaded.GapMinMinutes, loaded.GapMaxMinutes));
    }

    [Fact]
    public void RoundTrip_PreservesEveryField()
    {
        var saved = new FlashSettings
        {
            Enabled = false,
            Categories = [VerseCategory.Psalms, VerseCategory.PaulsEpistles],
            ReadingSpeed = ReadingSpeed.Fast,
            GapMinMinutes = 2,
            GapMaxMinutes = 30,
            FontFamily = "Georgia",
            FontSize = 40,
            StartWithWindows = true,
        };

        _store.Save(saved);
        var loaded = _store.Load(s_anyFont);

        Assert.False(loaded.Enabled);
        Assert.Equal([VerseCategory.Psalms, VerseCategory.PaulsEpistles], loaded.Categories);
        Assert.Equal(ReadingSpeed.Fast, loaded.ReadingSpeed);
        Assert.Equal((2, 30), (loaded.GapMinMinutes, loaded.GapMaxMinutes));
        Assert.Equal(("Georgia", 40), (loaded.FontFamily, loaded.FontSize));
        Assert.True(loaded.StartWithWindows);
    }

    [Fact]
    public void Save_CreatesDirectory_AndLeavesNoTempFile()
    {
        _store.Save(new FlashSettings());

        Assert.True(File.Exists(_path));
        Assert.Equal([_path], Directory.GetFiles(_directory));
    }

    [Fact]
    public void Save_OverwritesExistingFile()
    {
        _store.Save(new FlashSettings { FontSize = 30 });
        _store.Save(new FlashSettings { FontSize = 50 });

        Assert.Equal(50, _store.Load(s_anyFont).FontSize);
    }

    [Fact]
    public void MissingProperty_KeepsItsDefault()
    {
        WriteRaw("""{ "fontSize": 40 }""");

        var settings = _store.Load(s_anyFont);

        Assert.Equal(40, settings.FontSize);
        Assert.Equal(15, settings.GapMaxMinutes);
    }

    [Fact]
    public void NumericCategory_IsRejected()
    {
        WriteRaw("""{ "categories": [99], "fontSize": 40 }""");

        var settings = _store.Load(s_anyFont);

        Assert.Equal(28, settings.FontSize);
        Assert.Equal(6, settings.Categories.Count);
    }

    [Fact]
    public void OldFileWithDisplaySeconds_StillLoads()
    {
        WriteRaw("""{ "displayMinSeconds": 5, "displayMaxSeconds": 10, "fontSize": 40 }""");

        Assert.Equal(40, _store.Load(s_anyFont).FontSize);
    }

    [Fact]
    public void OversizedFile_LoadsDefaults()
    {
        WriteRaw("""{ "fontSize": 40, "pad": """ + "\"" + new string('x', 100_000) + "\" }");

        Assert.Equal(28, _store.Load(s_anyFont).FontSize);
    }
}

public class FlashSettingsTests
{
    private static readonly Func<string, bool> s_anyFont = _ => true;

    [Fact]
    public void Normalize_ClampsToLimits()
    {
        var settings = new FlashSettings
        {
            GapMinMinutes = 0,
            GapMaxMinutes = 10_000,
            FontSize = 500,
        }.Normalize(s_anyFont);

        Assert.Equal((1, 240), (settings.GapMinMinutes, settings.GapMaxMinutes));
        Assert.Equal(96, settings.FontSize);
        Assert.Equal(12, (new FlashSettings { FontSize = -4 }).Normalize(s_anyFont).FontSize);
    }

    [Fact]
    public void Normalize_SwapsInvertedRanges()
    {
        var settings = new FlashSettings
        {
            GapMinMinutes = 20,
            GapMaxMinutes = 3,
        }.Normalize(s_anyFont);

        Assert.Equal((3, 20), (settings.GapMinMinutes, settings.GapMaxMinutes));
    }

    [Fact]
    public void Normalize_UnknownFont_FallsBackToDefault()
    {
        var settings = new FlashSettings { FontFamily = "No Such Font" }.Normalize(name => name == "Segoe UI");

        Assert.Equal("Segoe UI", settings.FontFamily);
    }

    [Fact]
    public void Normalize_RemovesDuplicateAndUndefinedCategories()
    {
        var settings = new FlashSettings
        {
            Categories = [VerseCategory.Psalms, VerseCategory.Psalms, (VerseCategory)42],
        }.Normalize(s_anyFont);

        Assert.Equal([VerseCategory.Psalms], settings.Categories);
    }

    [Fact]
    public void Normalize_AllowsNoCategories()
    {
        var settings = new FlashSettings { Categories = [] }.Normalize(s_anyFont);

        Assert.Empty(settings.Categories);
    }

    [Fact]
    public void Normalize_UndefinedReadingSpeed_FallsBackToNormal() =>
        Assert.Equal(ReadingSpeed.Normal, new FlashSettings { ReadingSpeed = (ReadingSpeed)7 }.Normalize(s_anyFont).ReadingSpeed);

    [Fact]
    public void Normalize_DoesNotMutateInput()
    {
        var original = new FlashSettings { FontSize = 500 };

        _ = original.Normalize(s_anyFont);

        Assert.Equal(500, original.FontSize);
    }
}
