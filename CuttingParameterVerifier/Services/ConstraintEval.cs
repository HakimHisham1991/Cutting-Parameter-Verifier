using System.Linq;
using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>Point-in-polygon checks and status aggregation used by evaluation and chart coloring.</summary>
public static class ConstraintEval
{
    public static PassFailNa EvaluateCutting(CuttingDataRow row, IReadOnlyList<Point2D> poly)
    {
        if (poly.Count < 3) return PassFailNa.Na;
        if (row.SurfaceSpeedVcMMin is null || row.FeedPerToothFzMm is null) return PassFailNa.Na;
        var p = new Point2D(row.SurfaceSpeedVcMMin.Value, row.FeedPerToothFzMm.Value);
        return PolygonGeometry.IsInsideInclusive(poly, p) ? PassFailNa.Pass : PassFailNa.Fail;
    }

    public static PassFailNa EvaluateEngagement(CuttingDataRow row, IReadOnlyList<Point2D> poly)
    {
        if (poly.Count < 3) return PassFailNa.Na;
        if (row.AxialDocApMm is null || row.RadialDocAeMm is null) return PassFailNa.Na;
        var p = new Point2D(row.AxialDocApMm.Value, row.RadialDocAeMm.Value);
        return PolygonGeometry.IsInsideInclusive(poly, p) ? PassFailNa.Pass : PassFailNa.Fail;
    }

    /// <summary>Across multiple graphs: any Fail → Fail; else any N/A → N/A; else all Pass → Pass.</summary>
    public static PassFailNa AggregateAcrossGraphs(IReadOnlyList<PassFailNa> statuses)
    {
        if (statuses.Count == 0) return PassFailNa.Na;
        if (statuses.Any(s => s == PassFailNa.Fail)) return PassFailNa.Fail;
        if (statuses.Any(s => s == PassFailNa.Na)) return PassFailNa.Na;
        return PassFailNa.Pass;
    }
}
