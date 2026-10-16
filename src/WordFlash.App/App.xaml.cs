using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using WordFlash.Core;

namespace WordFlash.App;

public partial class App : Application
{
    private const string SingleInstanceMutexName = @"Local\WordFlash.SingleInstance";

    private readonly SettingsStore _settingsStore = new(SettingsStore.DefaultPath);
    private Mutex? _singleInstance;
    private TaskbarIcon? _trayIcon;
    private FlashSettings _settings = new();
    private FlashController? _controller;
    private SettingsWindow? _settingsWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstance = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            _singleInstance.Dispose();
            _singleInstance = null;
            Shutdown();
            return;
        }

        _settings = _settingsStore.Load(FontExists);

        _controller = new FlashController(VerseCatalog.LoadEmbedded(), new OverlayWindow(), _settings);
        _controller.UpdateSettings(_settings);

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _trayIcon.ForceCreate(enablesEfficiencyMode: false);
        FlashingMenuItem.IsChecked = _settings.Enabled;
        ApplyStartupRegistration(_settings.StartWithWindows);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _controller?.Stop();
        _trayIcon?.Dispose();
        if (_singleInstance is not null)
        {
            _singleInstance.ReleaseMutex();
            _singleInstance.Dispose();
        }

        base.OnExit(e);
    }

    internal static bool FontExists(string family) =>
        Fonts.SystemFontFamilies.Any(f => string.Equals(f.Source, family, StringComparison.OrdinalIgnoreCase));

    private MenuItem FlashingMenuItem => (MenuItem)_trayIcon!.ContextMenu.Items[0];

    private void ApplySettings(FlashSettings settings)
    {
        _settings = settings;
        try
        {
            _settingsStore.Save(settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            System.Diagnostics.Debug.WriteLine($"WordFlash: could not save settings: {ex.Message}");
        }

        FlashingMenuItem.IsChecked = settings.Enabled;
        _settingsWindow?.SyncEnabled(settings.Enabled);
        _controller?.UpdateSettings(settings);
        ApplyStartupRegistration(settings.StartWithWindows);
    }

    private void ApplyStartupRegistration(bool enabled)
    {
        if (!StartupRegistration.Apply(enabled))
        {
            _trayIcon?.ShowNotification(
                "Word-flash",
                "Couldn't update the Windows startup entry, so \"Start with Windows\" may not take effect.",
                NotificationIcon.Warning);
        }
    }

    private void OnFlashingClick(object sender, RoutedEventArgs e) =>
        ApplySettings(_settings with { Enabled = FlashingMenuItem.IsChecked });

    private void OnFlashNowClick(object sender, RoutedEventArgs e) => _controller?.FlashNow();

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_settings);
        _settingsWindow.Saved += (_, settings) => ApplySettings(settings);
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private void OnExitClick(object sender, RoutedEventArgs e) => Shutdown();
}
