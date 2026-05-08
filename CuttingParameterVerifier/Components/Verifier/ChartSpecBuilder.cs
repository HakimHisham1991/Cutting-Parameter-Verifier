using System.Globalization;
using CuttingParameterVerifier.Models;
using CuttingParameterVerifier.Services;

namespace CuttingParameterVerifier.Components.Verifier;

internal static class ChartSpecBuilder
{
    public static object Build(VerificationConfig cfg, IReadOnlyList<ResultRow> results)
    {
        var panels = new List<object>();
        foreach (var graph in cfg.Graphs.OrderBy(g => g.GraphNumber, StringComparer.OrdinalIgnoreCase))
        {
            var subset = results.Where(r => r.GraphNumber != null &&
                                            string.Equals(r.GraphNumber.Trim(), graph.GraphNumber.Trim(),
                                                StringComparison.Ordinal)).ToList();

            var mappingContext = BuildMappingContextSubtitle(cfg, graph.GraphNumber);
            panels.Add(new
            {
                graphNumber = graph.GraphNumber,
                cutCanvasId = GraphDomIds.Cutting(graph.GraphNumber),
                engCanvasId = GraphDomIds.Engagement(graph.GraphNumber),
                mappingContext,
                cutting = BuildCuttingSide(graph.CuttingPolygon, subset),
                engagement = BuildEngagementSide(graph.EngagementPolygon, subset)
            });
        }

        return new { panels };
    }

    /// <summary>Subtitle: Material | Surface | Milling | Tool | Strategy — one line per mapping rule.</summary>
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
                var parts = new[] { T(r.Material), T(r.SurfaceType), T(r.MillingType), T(r.ToolType), T(r.StrategyType) }
                    .Where(p => p.Length > 0);
                return string.Join(" | ", parts);
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return string.Join("\n", blocks);
    }

    private static object BuildCuttingSide(IReadOnlyList<Point2D> polygon, IReadOnlyList<ResultRow> subset)
    {
        var pass = new List<object>();
        var fail = new List<object>();
        var na = new List<object>();

        foreach (var r in subset)
        {
            var status = r.ParameterStatus;
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
            var status = r.EngagementStatus;
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
