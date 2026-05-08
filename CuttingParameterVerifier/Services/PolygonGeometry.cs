using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public static class PolygonGeometry
{
    private const double Epsilon = 1e-12;

    /// <summary>Removes consecutive duplicate vertices (safe for ray-casting).</summary>
    public static List<Point2D> DeduplicateConsecutive(IReadOnlyList<Point2D> vertices)
    {
        if (vertices.Count == 0) return new List<Point2D>();

        var result = new List<Point2D> { vertices[0] };
        for (var i = 1; i < vertices.Count; i++)
        {
            var p = vertices[i];
            var last = result[^1];
            if (Math.Abs(p.X - last.X) < Epsilon && Math.Abs(p.Y - last.Y) < Epsilon)
                continue;
            result.Add(p);
        }

        if (result.Count >= 2)
        {
            var first = result[0];
            var end = result[^1];
            if (Math.Abs(first.X - end.X) < Epsilon && Math.Abs(first.Y - end.Y) < Epsilon)
                result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    /// <summary>Ray-casting point-in-polygon; boundary treated as inside using inclusive edge test.</summary>
    public static bool IsInsideInclusive(IReadOnlyList<Point2D> polygon, Point2D p)
    {
        if (polygon.Count < 3) return false;

        var x = p.X;
        var y = p.Y;
        var inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            var xi = polygon[i].X;
            var yi = polygon[i].Y;
            var xj = polygon[j].X;
            var yj = polygon[j].Y;

            if (PointOnSegmentInclusive(x, y, xi, yi, xj, yj))
                return true;

            var intersect = (yi > y) != (yj > y) &&
                            x < (xj - xi) * (y - yi) / (yj - yi + Epsilon) + xi;
            if (intersect) inside = !inside;
        }

        return inside;
    }

    private static bool PointOnSegmentInclusive(double px, double py, double ax, double ay, double bx, double by)
    {
        var cross = (py - ay) * (bx - ax) - (px - ax) * (by - ay);
        if (Math.Abs(cross) > 1e-9) return false;
        var dot = (px - ax) * (bx - ax) + (py - ay) * (by - ay);
        if (dot < -1e-9) return false;
        var len2 = (bx - ax) * (bx - ax) + (by - ay) * (by - ay);
        return dot <= len2 + 1e-9;
    }
}
