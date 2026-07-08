using System.Globalization;
using System.Linq;
using CuttingParameterVerifier.Models;
using CuttingParameterVerifier.Services;

namespace CuttingParameterVerifier.Components.Verifier;

internal static class ChartSpecBuilder
{
    public static object Build(VerificationConfig cfg, IReadOnlyList<ResultRow> results) =>
        Build(cfg, results, canvasIdPrefix: null, graphNumberFilter: null);

    /// <param name="canvasIdPrefix">Optional DOM id prefix (e.g. settings preview).</param>
    /// <param name="graphNumberFilter">When set, only build the panel for this graph number.</param>
    public static object Build(
        VerificationConfig cfg,
        IReadOnlyList<ResultRow> results,
        string? canvasIdPrefix,
        string? graphNumberFilter)
    {
        var panels = new List<object>();
        IEnumerable<ConstraintGraph> graphs = cfg.Graphs.OrderBy(g => g.GraphNumber, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(graphNumberFilter))
        {
            var key = graphNumberFilter.Trim();
            graphs = graphs.Where(g => string.Equals(g.GraphNumber.Trim(), key, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var graph in graphs)
        {
            var subset = results.Where(r => RowMatchesGraph(r, graph.GraphNumber.Trim())).ToList();
            var cutPoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.CuttingPolygon);
            var mappingContext = BuildMappingContextSubtitle(cfg, graph.GraphNumber);

            if (graph.EngagementMode == EngagementMode.DiameterScaled)
            {
                DiameterInequalityService.EnsureInequalities(graph);
                var diameters = subset.Select(r => r.Source.DiameterMm ?? 0).ToList();
                var plotMaxX = DiameterInequalityService.ResolvePlotMaxMm(graph, diameters);
                panels.Add(new
                {
                    graphNumber = graph.GraphNumber,
                    engagementMode = "diameterScaled",
                    cutCanvasId = GraphDomIds.Cutting(graph.GraphNumber, canvasIdPrefix),
                    engAeCanvasId = GraphDomIds.EngagementAeVsDiameter(graph.GraphNumber, canvasIdPrefix),
                    engApCanvasId = GraphDomIds.EngagementApVsDiameter(graph.GraphNumber, canvasIdPrefix),
                    mappingContext,
                    cutting = BuildCuttingSide(cutPoly, subset),
                    engagementAeVsDiameter = BuildEngagementAeVsDiameterSide(graph, plotMaxX, subset),
                    engagementApVsDiameter = BuildEngagementApVsDiameterSide(graph, plotMaxX, subset)
                });
            }
            else
            {
                var engPoly = PolygonNormalizer.EnsureEvaluablePolygon(graph.EngagementPolygon);
                panels.Add(new
                {
                    graphNumber = graph.GraphNumber,
                    engagementMode = "apAe",
                    cutCanvasId = GraphDomIds.Cutting(graph.GraphNumber, canvasIdPrefix),
                    engCanvasId = GraphDomIds.Engagement(graph.GraphNumber, canvasIdPrefix),
                    mappingContext,
                    cutting = BuildCuttingSide(cutPoly, subset),
                    engagement = BuildEngagementSide(engPoly, subset)
                });
            }
        }

        return new { panels };
    }

    /// <summary>Subtitle: Process Specs | Material | Surface | Milling | Tool | Strategy — one line per mapping rule.</summary>
    private static string BuildMappingContextSubtitle(VerificationConfig cfg, string graphNumber)
    {
        var key = graphNumber.Trim();
        var rules = cfg.MappingRules
            .Where(r => string.Equals(r.GraphNumber.Trim(), key, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (rules.Count == 0)
            return "";

        static string T(string? s) => (s ?? "").Trim();

        var blocks = rules
            .Select(r =>
            {
                var parts = new[] { T(r.ProcessSpecs), T(r.Material), T(r.SurfaceType), T(r.MillingType), T(r.ToolType), T(r.StrategyType) }
                    .Where(p => p.Length > 0);
                return string.Join(" | ", parts);
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return string.Join("\n", blocks);
    }

    private static bool RowMatchesGraph(ResultRow r, string graphNumber) =>
        r.MatchedGraphNumbers is not null &&
        r.MatchedGraphNumbers.Any(id =>
            string.Equals(id.Trim(), graphNumber.Trim(), StringComparison.OrdinalIgnoreCase));

    private static object BuildCuttingSide(IReadOnlyList<Point2D> polygon, IReadOnlyList<ResultRow> subset)
    {
        var pass = new List<object>();
        var fail = new List<object>();
        var na = new List<object>();

        foreach (var r in subset)
        {
            var status = ConstraintEval.EvaluateCutting(r.Source, polygon);
            if (r.Source.SurfaceSpeedVcMMin is null || r.Source.FeedPerToothFzMm is null)
                continue;

            var x = r.Source.SurfaceSpeedVcMMin.Value;
            var y = r.Source.FeedPerToothFzMm.Value;
            Bucket(status, ScatterPoint(x, y, status, r.Source.No), pass, fail, na);
        }

        return new
        {
            polygon = polygon.Select(p => new { x = p.X, y = p.Y }).ToList(),
            pass,
            fail,
            na
        };
    }

    private static object BuildEngagementSide(IReadOnlyList<Point2D> polygon, IReadOnlyList<ResultRow> subset)
    {
        var pass = new List<object>();
        var fail = new List<object>();
        var na = new List<object>();

        foreach (var r in subset)
        {
            var status = ConstraintEval.EvaluateEngagement(r.Source, polygon);
            if (r.Source.AxialDocApMm is null || r.Source.RadialDocAeMm is null)
                continue;

            var x = r.Source.AxialDocApMm.Value;
            var y = r.Source.RadialDocAeMm.Value;
            Bucket(status, ScatterPoint(x, y, status, r.Source.No), pass, fail, na);
        }

        return new
        {
            polygon = polygon.Select(p => new { x = p.X, y = p.Y }).ToList(),
            pass,
            fail,
            na
        };
    }

    private static object BuildEngagementAeVsDiameterSide(ConstraintGraph graph, double plotMaxX, IReadOnlyList<ResultRow> subset)
    {
        var pass = new List<object>();
        var fail = new List<object>();
        var na = new List<object>();

        foreach (var r in subset)
        {
            var status = ConstraintEval.EvaluateEngagementAeVsDiameter(r.Source, graph);
            if (r.Source.DiameterMm is null or <= 0 || r.Source.RadialDocAeMm is null)
                continue;

            var x = r.Source.DiameterMm.Value;
            var y = r.Source.RadialDocAeMm.Value;
            Bucket(status, ScatterPoint(x, y, status, r.Source.No), pass, fail, na);
        }

        var parsed = DiameterInequalityService.ParseAe(graph);
        var dataY = subset.Where(r => r.Source.RadialDocAeMm is not null).Select(r => r.Source.RadialDocAeMm!.Value);
        return BuildDiameterScaledSide(parsed, plotMaxX, dataY, pass, fail, na);
    }

    private static object BuildEngagementApVsDiameterSide(ConstraintGraph graph, double plotMaxX, IReadOnlyList<ResultRow> subset)
    {
        var pass = new List<object>();
        var fail = new List<object>();
        var na = new List<object>();

        foreach (var r in subset)
        {
            var status = ConstraintEval.EvaluateEngagementApVsDiameter(r.Source, graph);
            if (r.Source.DiameterMm is null or <= 0 || r.Source.AxialDocApMm is null)
                continue;

            var x = r.Source.DiameterMm.Value;
            var y = r.Source.AxialDocApMm.Value;
            Bucket(status, ScatterPoint(x, y, status, r.Source.No), pass, fail, na);
        }

        var parsed = DiameterInequalityService.ParseAp(graph);
        var dataY = subset.Where(r => r.Source.AxialDocApMm is not null).Select(r => r.Source.AxialDocApMm!.Value);
        return BuildDiameterScaledSide(parsed, plotMaxX, dataY, pass, fail, na);
    }

    private static object BuildDiameterScaledSide(
        IReadOnlyList<DiameterInequalityParser.ParsedConstraint> parsed,
        double plotMaxX,
        IEnumerable<double> dataValuesMm,
        List<object> pass,
        List<object> fail,
        List<object> na)
    {
        var plotMaxY = DiameterInequalityService.ResolvePlotMaxYMm(parsed, plotMaxX, dataValuesMm);
        var polygon = DiameterInequalityService.BuildPassRegionPolygon(parsed, plotMaxX, plotMaxY);
        var lines = DiameterInequalityService.BuildBoundaryLines(parsed, plotMaxX);

        return new
        {
            polygon = polygon.Select(p => new { x = p.X, y = p.Y }).ToList(),
            inequalityLines = lines.Select(l => new
            {
                label = l.Label,
                points = l.Points.Select(p => new { x = p.X, y = p.Y }).ToList()
            }).ToList(),
            pass,
            fail,
            na
        };
    }

    private static object ScatterPoint(double x, double y, PassFailNa status, int? rowNo)
    {
        var outcome = OutcomeLabel(status);
        var rowPart = rowNo.HasValue ? $"No. {rowNo.Value}" : "No. —";
        var coords = $"{FormatTooltipCoord(x)}, {FormatTooltipCoord(y)}";
        var tooltip = $"{outcome}: {rowPart} ({coords})";
        return new { x, y, tooltip };
    }

    private static string OutcomeLabel(PassFailNa s) => s switch
    {
        PassFailNa.Pass => "Pass",
        PassFailNa.Fail => "Fail",
        _ => "N/A"
    };

    private static string FormatTooltipCoord(double v)
    {
        var inv = CultureInfo.InvariantCulture;
        return Math.Abs(v) >= 1.0 ? v.ToString("0.##", inv) : v.ToString("0.####", inv);
    }

    private static void Bucket(PassFailNa status, object pt, List<object> pass, List<object> fail, List<object> na)
    {
        switch (status)
        {
            case PassFailNa.Pass:
                pass.Add(pt);
                break;
            case PassFailNa.Fail:
                fail.Add(pt);
                break;
            default:
                na.Add(pt);
                break;
        }
    }
}
