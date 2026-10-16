using System.Drawing;

namespace WordFlash.Core;

/// <summary>
/// Placement maths for the overlay. All values are in the same units as the work area
/// (WPF device-independent pixels in the app).
/// </summary>
public static class OverlayLayout
{
    private const float SideWidthFraction = 0.40f;
    private const float WideWidthFraction = 0.70f;

    public static float MaxBoxWidth(RectangleF workArea, OverlayPosition position) =>
        workArea.Width * (position is OverlayPosition.Left or OverlayPosition.Right ? SideWidthFraction : WideWidthFraction);

    public static PointF Place(RectangleF workArea, SizeF box, OverlayPosition position, float margin)
    {
        var centreX = workArea.X + ((workArea.Width - box.Width) / 2);
        var centreY = workArea.Y + ((workArea.Height - box.Height) / 2);

        var (x, y) = position switch
        {
            OverlayPosition.Top => (centreX, workArea.Y + margin),
            OverlayPosition.Bottom => (centreX, workArea.Bottom - margin - box.Height),
            OverlayPosition.Left => (workArea.X + margin, centreY),
            OverlayPosition.Right => (workArea.Right - margin - box.Width, centreY),
            _ => (centreX, centreY),
        };

        return new PointF(
            Math.Max(workArea.X, Math.Min(x, workArea.Right - box.Width)),
            Math.Max(workArea.Y, Math.Min(y, workArea.Bottom - box.Height)));
    }
}
