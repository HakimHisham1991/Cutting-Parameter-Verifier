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

    public static PassFailNa EvaluateEngagementAeVsDiameter(CuttingDataRow row, IReadOnlyList<Point2D> poly)
    {
        if (poly.Count < 3) return PassFailNa.Na;
        if (row.DiameterMm is null or <= 0 || row.RadialDocAeMm is null) return PassFailNa.Na;
        var p = new Point2D(row.DiameterMm.Value, row.RadialDocAeMm.Value);
        return PolygonGeometry.IsInsideInclusive(poly, p) ? PassFailNa.Pass : PassFailNa.Fail;
    }

    public static PassFailNa EvaluateEngagementApVsDiameter(CuttingDataRow row, IReadOnlyList<Point2D> poly)
    {
        if (poly.Count < 3) return PassFailNa.Na;
        if (row.DiameterMm is null or <= 0 || row.AxialDocApMm is null) return PassFailNa.Na;
        var p = new Point2D(row.DiameterMm.Value, row.AxialDocApMm.Value);
        return PolygonGeometry.IsInsideInclusive(poly, p) ? PassFailNa.Pass : PassFailNa.Fail;
    }

    /// <summary>Evaluates engagement for a graph using its configured mode.</summary>
    public static PassFailNa EvaluateEngagementForGraph(CuttingDataRow row, ConstraintGraph graph)
    {
        if (graph.EngagementMode == EngagementMode.DiameterScaled)
        {
            var aePoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.EngagementAeVsDiameterPolygon);
            var apPoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.EngagementApVsDiameterPolygon);
            return AggregateAcrossGraphs(new[]
            {
                EvaluateEngagementAeVsDiameter(row, aePoly),
                EvaluateEngagementApVsDiameter(row, apPoly)
            });
        }

        var engPoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.EngagementPolygon);
        return EvaluateEngagement(row, engPoly);
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
