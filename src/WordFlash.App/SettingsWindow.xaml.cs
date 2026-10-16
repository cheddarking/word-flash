using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WordFlash.Core;

namespace WordFlash.App;

public partial class SettingsWindow : Window
{
    private static readonly ReadingSpeedOption[] s_readingSpeeds =
    [
        new(ReadingSpeed.Slow, "Slow (150 words/min)"),
        new(ReadingSpeed.Normal, "Normal (180 words/min)"),
        new(ReadingSpeed.Fast, "Fast (220 words/min)"),
    ];

    private readonly FlashSettings _original;
    private readonly Dictionary<VerseCategory, CheckBox> _categoryBoxes;

    public SettingsWindow(FlashSettings current)
    {
        InitializeComponent();
        _original = current;
        _categoryBoxes = new()
        {
            [VerseCategory.JesusWords] = JesusWordsBox,
            [VerseCategory.Proverbs] = ProverbsBox,
            [VerseCategory.Psalms] = PsalmsBox,
            [VerseCategory.Torah] = TorahBox,
            [VerseCategory.Prophets] = ProphetsBox,
            [VerseCategory.PaulsEpistles] = PaulsEpistlesBox,
        };

        EnabledBox.IsChecked = current.Enabled;
        foreach (var (category, box) in _categoryBoxes)
        {
            box.IsChecked = current.Categories.Contains(category);
        }

        ReadingSpeedBox.ItemsSource = s_readingSpeeds;
        ReadingSpeedBox.DisplayMemberPath = nameof(ReadingSpeedOption.Label);
        ReadingSpeedBox.SelectedItem = s_readingSpeeds.Single(o => o.Speed == current.ReadingSpeed);
        GapMinBox.Text = current.GapMinMinutes.ToString();
        GapMaxBox.Text = current.GapMaxMinutes.ToString();
        FontBox.ItemsSource = Fonts.SystemFontFamilies.Select(f => f.Source).Order(StringComparer.CurrentCultureIgnoreCase).ToList();
        FontBox.SelectedItem = current.FontFamily;
        SizeSlider.Value = current.FontSize;
        StartWithWindowsBox.IsChecked = current.StartWithWindows;
    }

    public event EventHandler<FlashSettings>? Saved;

    /// <summary>
    /// Mirrors an on/off change made from the tray while this window is open, so Save doesn't revert it.
    /// </summary>
    public void SyncEnabled(bool enabled) => EnabledBox.IsChecked = enabled;

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var numbers = new[] { GapMinBox, GapMaxBox };
        var invalid = numbers.FirstOrDefault(box => !int.TryParse(box.Text, out _));
        if (invalid is not null)
        {
            MessageBox.Show(this, "Please enter whole numbers of minutes.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            invalid.Focus();
            invalid.SelectAll();
            return;
        }

        var settings = _original with
        {
            Enabled = EnabledBox.IsChecked == true,
            Categories = _categoryBoxes.Where(pair => pair.Value.IsChecked == true).Select(pair => pair.Key).ToArray(),
            ReadingSpeed = (ReadingSpeedBox.SelectedItem as ReadingSpeedOption)?.Speed ?? _original.ReadingSpeed,
            GapMinMinutes = int.Parse(GapMinBox.Text),
            GapMaxMinutes = int.Parse(GapMaxBox.Text),
            FontFamily = FontBox.SelectedItem as string ?? _original.FontFamily,
            FontSize = (int)SizeSlider.Value,
            StartWithWindows = StartWithWindowsBox.IsChecked == true,
        };

        Saved?.Invoke(this, settings.Normalize(App.FontExists));
        Close();
    }

    private sealed record ReadingSpeedOption(ReadingSpeed Speed, string Label);
}
