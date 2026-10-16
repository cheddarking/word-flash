using System.Windows.Threading;
using WordFlash.Core;

namespace WordFlash.App;

/// <summary>
/// Runs the flash loop: wait a random gap, show a verse, wait for it to finish, repeat.
/// </summary>
public sealed class FlashController
{
    private readonly VerseCatalog _catalog;
    private readonly OverlayWindow _overlay;
    private readonly Random _rng = new();
    private readonly DispatcherTimer _nextTimer = new();
    private FlashSettings _settings;
    private FlashPlan? _pending;

    public FlashController(VerseCatalog catalog, OverlayWindow overlay, FlashSettings settings)
    {
        _catalog = catalog;
        _overlay = overlay;
        _settings = settings;
        _nextTimer.Tick += (_, _) => OnNextTimer();
    }

    /// <summary>
    /// Applies new settings and restarts the countdown so a changed gap takes effect at once.
    /// </summary>
    public void UpdateSettings(FlashSettings settings)
    {
        _settings = settings;
        if (!settings.Enabled)
        {
            _overlay.HideFlash();
        }

        ScheduleNext(TimeSpan.Zero);
    }

    public void FlashNow()
    {
        var plan = FlashScheduler.NextFlash(_settings, _catalog, _rng);
        if (plan is not null)
        {
            _overlay.ShowFlash(plan, _settings);
        }
    }

    public void Stop()
    {
        _nextTimer.Stop();
        _pending = null;
    }

    private void ScheduleNext(TimeSpan afterCurrentFlash)
    {
        _nextTimer.Stop();
        _pending = _settings.Enabled ? FlashScheduler.NextFlash(_settings, _catalog, _rng) : null;
        if (_pending is null)
        {
            return;
        }

        _nextTimer.Interval = afterCurrentFlash + _pending.Delay;
        _nextTimer.Start();
    }

    private void OnNextTimer()
    {
        _nextTimer.Stop();
        if (_pending is null || !_settings.Enabled)
        {
            return;
        }

        var plan = _pending;
        _overlay.ShowFlash(plan, _settings);
        ScheduleNext(plan.Duration);
    }
}
