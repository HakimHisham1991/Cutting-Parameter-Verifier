using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>Expands degenerate 1–2 vertex inputs into a minimal valid polygon for evaluation and settings save.</summary>
public static class PolygonNormalizer
{
    public static List<Point2D> EnsureEvaluablePolygon(IReadOnlyList<Point2D> raw)
    {
        var pts = PolygonGeometry.DeduplicateConsecutive(raw);
        if (pts.Count >= 3)
            return pts;

        if (pts.Count == 2)
        {
            var ax = pts[0].X;
            var ay = pts[0].Y;
            var bx = pts[1].X;
            var by = pts[1].Y;
            var dx = bx - ax;
            var dy = by - ay;
            var len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-12)
                return SquareAround(ax, ay);

            var nx = -dy / len;
            var ny = dx / len;
            var off = Math.Max(1e-6, 1e-4 * (Math.Abs(ax) + Math.Abs(ay) + Math.Abs(bx) + Math.Abs(by) + 1.0));
            return new List<Point2D>
            {
                new(ax, ay),
                new(bx, by),
                new(bx + nx * off, by + ny * off),
                new(ax + nx * off, ay + ny * off)
            };
        }

        if (pts.Count == 1)
            return SquareAround(pts[0].X, pts[0].Y);

        return pts;
    }

    private static List<Point2D> SquareAround(double x, double y)
    {
        var e = Math.Max(1e-4, 1e-3 * (Math.Abs(x) + Math.Abs(y) + 1.0));
        return new List<Point2D>
        {
            new(x, y),
            new(x + e, y),
            new(x + e, y + e),
            new(x, y + e)
        };
    }
}
