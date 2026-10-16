using System.Drawing;
using WordFlash.Core;

namespace WordFlash.Tests;

public class OverlayLayoutTests
{
    private const float Margin = 40;

    public static TheoryData<RectangleF, SizeF> Screens => new()
    {
        { new RectangleF(0, 0, 1920, 1040), new SizeF(700, 160) },
        { new RectangleF(0, 0, 1280, 693.33f), new SizeF(500, 140) },
        { new RectangleF(-1280, 0, 1280, 693.33f), new SizeF(500, 140) },
    };

    [Theory]
    [MemberData(nameof(Screens))]
    public void AllPositions_InsideWorkArea(RectangleF workArea, SizeF box)
    {
        foreach (var position in Enum.GetValues<OverlayPosition>())
        {
            var topLeft = OverlayLayout.Place(workArea, box, position, Margin);

            Assert.True(workArea.Contains(new RectangleF(topLeft, box)), $"{position} at {topLeft} leaves {workArea}");
        }
    }

    [Fact]
    public void EdgePositions_SitOnTheirEdgeAndAreCentredAlongIt()
    {
        var area = new RectangleF(0, 0, 1920, 1040);
        var box = new SizeF(600, 100);

        Assert.Equal(new PointF(660, 40), OverlayLayout.Place(area, box, OverlayPosition.Top, Margin));
        Assert.Equal(new PointF(660, 900), OverlayLayout.Place(area, box, OverlayPosition.Bottom, Margin));
        Assert.Equal(new PointF(40, 470), OverlayLayout.Place(area, box, OverlayPosition.Left, Margin));
        Assert.Equal(new PointF(1280, 470), OverlayLayout.Place(area, box, OverlayPosition.Right, Margin));
        Assert.Equal(new PointF(660, 470), OverlayLayout.Place(area, box, OverlayPosition.Center, Margin));
    }

    [Fact]
    public void BoxLargerThanWorkArea_IsPinnedToTopLeft()
    {
        var area = new RectangleF(100, 50, 400, 300);

        var topLeft = OverlayLayout.Place(area, new SizeF(900, 900), OverlayPosition.Right, Margin);

        Assert.Equal(new PointF(100, 50), topLeft);
    }

    [Theory]
    [InlineData(OverlayPosition.Left, 0.40f)]
    [InlineData(OverlayPosition.Right, 0.40f)]
    [InlineData(OverlayPosition.Top, 0.70f)]
    [InlineData(OverlayPosition.Bottom, 0.70f)]
    [InlineData(OverlayPosition.Center, 0.70f)]
    public void MaxBoxWidth_IsAFractionOfTheWorkArea(OverlayPosition position, float fraction) =>
        Assert.Equal(1000 * fraction, OverlayLayout.MaxBoxWidth(new RectangleF(0, 0, 1000, 800), position), 3);
}
