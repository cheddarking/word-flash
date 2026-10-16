using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WordFlash.Core;

namespace WordFlash.App;

public partial class OverlayWindow : Window
{
    private const float ScreenMargin = 40;
    private static readonly Duration s_fade = new(TimeSpan.FromMilliseconds(300));

    private readonly DispatcherTimer _hideTimer = new();

    public OverlayWindow()
    {
        InitializeComponent();
        _hideTimer.Tick += (_, _) => FadeOut();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        NativeMethods.MakeClickThrough(new WindowInteropHelper(this).Handle);
    }

    public void ShowFlash(FlashPlan plan, FlashSettings settings)
    {
        _hideTimer.Stop();

        var workArea = SystemParameters.WorkArea;
        var area = new RectangleF((float)workArea.X, (float)workArea.Y, (float)workArea.Width, (float)workArea.Height);

        var fontFamily = new System.Windows.Media.FontFamily(settings.FontFamily);
        VerseText.Text = plan.Verse.Text;
        VerseText.FontFamily = fontFamily;
        VerseText.FontSize = settings.FontSize;
        ReferenceText.Text = plan.Verse.Reference;
        ReferenceText.FontFamily = fontFamily;
        ReferenceText.FontSize = Math.Max(FlashSettings.MinFontSize, settings.FontSize * 0.6);
        Backdrop.MaxWidth = OverlayLayout.MaxBoxWidth(area, plan.Position);

        BeginAnimation(OpacityProperty, null);
        Opacity = 0;
        Show();
        Topmost = true;
        UpdateLayout();

        var topLeft = OverlayLayout.Place(area, new SizeF((float)ActualWidth, (float)ActualHeight), plan.Position, ScreenMargin);
        Left = topLeft.X;
        Top = topLeft.Y;

        BeginAnimation(OpacityProperty, new DoubleAnimation(1, s_fade));
        _hideTimer.Interval = plan.Duration;
        _hideTimer.Start();
    }

    public void HideFlash()
    {
        _hideTimer.Stop();
        BeginAnimation(OpacityProperty, null);
        Hide();
    }

    private void FadeOut()
    {
        _hideTimer.Stop();
        var fade = new DoubleAnimation(0, s_fade);
        fade.Completed += (_, _) =>
        {
            if (!_hideTimer.IsEnabled)
            {
                Hide();
            }
        };
        BeginAnimation(OpacityProperty, fade);
    }
}
