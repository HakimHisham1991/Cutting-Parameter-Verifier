using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class EvaluationService : IEvaluationService
{
    private readonly IMappingService _mapping;
    private readonly IConstraintService _constraints;

    public EvaluationService(IMappingService mapping, IConstraintService constraints)
    {
        _mapping = mapping;
        _constraints = constraints;
    }

    public IReadOnlyList<ResultRow> Evaluate(IReadOnlyList<CuttingDataRow> rows, VerificationConfig config)
    {
        var results = new List<ResultRow>(rows.Count);
        foreach (var row in rows)
        {
            if (!row.IsValid)
            {
                results.Add(new ResultRow
                {
                    Source = row,
                    GraphNumber = null,
                    ParameterStatus = PassFailNa.Na,
                    EngagementStatus = PassFailNa.Na
                });
                continue;
            }

            var graphNo = _mapping.ResolveGraphNumber(row, config);
            if (string.IsNullOrWhiteSpace(graphNo))
            {
                results.Add(new ResultRow
                {
                    Source = row,
                    GraphNumber = null,
                    ParameterStatus = PassFailNa.Na,
                    EngagementStatus = PassFailNa.Na
                });
                continue;
            }

            var graph = _constraints.FindGraph(config, graphNo);
            if (graph is null)
            {
                results.Add(new ResultRow
                {
                    Source = row,
                    GraphNumber = graphNo,
                    ParameterStatus = PassFailNa.Na,
                    EngagementStatus = PassFailNa.Na
                });
                continue;
            }

            var cutPoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.CuttingPolygon);
            var engPoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.EngagementPolygon);

            var param = EvaluateCutting(row, cutPoly);
            var eng = EvaluateEngagement(row, engPoly);

            results.Add(new ResultRow
            {
                Source = row,
                GraphNumber = graphNo,
                ParameterStatus = param,
                EngagementStatus = eng
            });
        }

        return results;
    }

    private static PassFailNa EvaluateCutting(CuttingDataRow row, IReadOnlyList<Point2D> poly)
    {
        if (poly.Count < 3) return PassFailNa.Na;
        if (row.SurfaceSpeedVcMMin is null || row.FeedPerToothFzMm is null) return PassFailNa.Na;
        var p = new Point2D(row.SurfaceSpeedVcMMin.Value, row.FeedPerToothFzMm.Value);
        return PolygonGeometry.IsInsideInclusive(poly, p) ? PassFailNa.Pass : PassFailNa.Fail;
    }

    private static PassFailNa EvaluateEngagement(CuttingDataRow row, IReadOnlyList<Point2D> poly)
    {
        if (poly.Count < 3) return PassFailNa.Na;
        if (row.AxialDocApMm is null || row.RadialDocAeMm is null) return PassFailNa.Na;
        var p = new Point2D(row.AxialDocApMm.Value, row.RadialDocAeMm.Value);
        return PolygonGeometry.IsInsideInclusive(poly, p) ? PassFailNa.Pass : PassFailNa.Fail;
    }
}
