using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace PresentationCursor;

public class TrailSegment
{
    public const double DefaultThickness = 10;
    public Line Line { get; }
    public double Age { get; set; }

    public TrailSegment(Point start, Point end)
    {
        Line = new Line
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
            Stroke = new SolidColorBrush(Colors.Yellow),
            StrokeThickness = DefaultThickness,
            Opacity = 1.0,
            IsHitTestVisible = false,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        Age = 0;
    }
}