using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>Evaluates and renders diameter-scaled ae/ap vs Ø inequality lists.</summary>
public static class DiameterInequalityService
{
    public const double DefaultPlotMaxMm = 50;
    private const double ClipEpsilon = 1e-9;

    public static PassFailNa Evaluate(double valueMm, double diameterMm, IReadOnlyList<DiameterInequalityParser.ParsedConstraint> constraints)
    {
        if (diameterMm <= 0 || constraints.Count == 0) return PassFailNa.Na;
        foreach (var c in constraints)
        {
            if (!c.Satisfies(diameterMm, valueMm)) return PassFailNa.Fail;
        }
        return PassFailNa.Pass;
    }

    public static List<Point2D> BuildPassRegionPolygon(
        IReadOnlyList<DiameterInequalityParser.ParsedConstraint> constraints,
        double xMaxMm,
        double yMaxMm)
    {
        if (constraints.Count == 0 || xMaxMm <= 0 || yMaxMm <= 0) return new List<Point2D>();

        var poly = new List<Point2D>
        {
            new(0, 0),
            new(xMaxMm, 0),
            new(xMaxMm, yMaxMm),
            new(0, yMaxMm)
        };

        foreach (var c in constraints)
        {
            var (a, b, cc) = c.ToHalfPlaneCoefficients();
            poly = ClipHalfPlane(poly, a, b, cc);
            if (poly.Count < 3) return new List<Point2D>();
        }

        return poly;
    }

    public static List<(string Label, List<Point2D> Points)> BuildBoundaryLines(
        IReadOnlyList<DiameterInequalityParser.ParsedConstraint> constraints,
        double xMaxMm)
    {
        var lines = new List<(string, List<Point2D>)>();
        if (xMaxMm <= 0) return lines;

        foreach (var c in constraints)
        {
            var y0 = c.BoundAt(0);
            var y1 = c.BoundAt(xMaxMm);
            lines.Add((c.RawExpression, new List<Point2D> { new(0, y0), new(xMaxMm, y1) }));
        }

        return lines;
    }

    public static double ResolvePlotMaxYMm(
        IReadOnlyList<DiameterInequalityParser.ParsedConstraint> constraints,
        double xMaxMm,
        IEnumerable<double> dataValuesMm)
    {
        var yMax = 10.0;
        foreach (var c in constraints)
        {
            yMax = Math.Max(yMax, c.BoundAt(0));
            yMax = Math.Max(yMax, c.BoundAt(xMaxMm));
        }

        var dataMax = dataValuesMm.Where(v => v > 0).DefaultIfEmpty(0).Max();
        if (dataMax > 0) yMax = Math.Max(yMax, dataMax * 1.1);
        return Math.Ceiling(yMax * 100) / 100;
    }

    public static double ResolvePlotMaxMm(ConstraintGraph graph, IEnumerable<double> dataDiametersMm)
    {
        var configured = graph.DiameterPlotMaxMm > 0 ? graph.DiameterPlotMaxMm : DefaultPlotMaxMm;
        var dataMax = dataDiametersMm.Where(d => d > 0).DefaultIfEmpty(0).Max();
        if (dataMax <= 0) return configured;
        return Math.Max(configured, Math.Ceiling(dataMax * 1.1 * 100) / 100);
    }

    public static List<DiameterInequalityParser.ParsedConstraint> ParseAe(ConstraintGraph graph) =>
        ParseList(graph.AeVsDiameterInequalities, "ae");

    public static List<DiameterInequalityParser.ParsedConstraint> ParseAp(ConstraintGraph graph) =>
        ParseList(graph.ApVsDiameterInequalities, "ap");

    private static List<DiameterInequalityParser.ParsedConstraint> ParseList(List<DiameterInequality> list, string variable)
    {
        var exprs = list.Select(i => i.Expression).Where(e => !string.IsNullOrWhiteSpace(e));
        DiameterInequalityParser.TryParseAll(exprs, variable, out var parsed, out _);
        return parsed;
    }

    public static void EnsureInequalities(ConstraintGraph graph)
    {
        if (graph.EngagementMode != EngagementMode.DiameterScaled) return;

        MigrateLegacyRanges(graph);

        if (graph.AeVsDiameterInequalities.Count == 0)
        {
            graph.AeVsDiameterInequalities.Add(new DiameterInequality { Expression = "ae >= 0" });
            graph.AeVsDiameterInequalities.Add(new DiameterInequality { Expression = "ae <= 1*D" });
        }

        if (graph.ApVsDiameterInequalities.Count == 0)
        {
            graph.ApVsDiameterInequalities.Add(new DiameterInequality { Expression = "ap >= 0.5*D" });
            graph.ApVsDiameterInequalities.Add(new DiameterInequality { Expression = "ap <= 1*D" });
        }

        if (graph.DiameterPlotMaxMm <= 0)
            graph.DiameterPlotMaxMm = DefaultPlotMaxMm;

        SyncPolygonsFromInequalities(graph);
    }

    private static void MigrateLegacyRanges(ConstraintGraph graph)
    {
        if (graph.AeVsDiameterInequalities.Count == 0 && graph.AeVsDiameterRange is not null)
        {
            var r = graph.AeVsDiameterRange;
            graph.AeVsDiameterInequalities.Add(new DiameterInequality
            {
                Expression = r.MinD == 0 ? "ae >= 0" : $"ae >= {FormatCoeff(r.MinD)}*D"
            });
            graph.AeVsDiameterInequalities.Add(new DiameterInequality
            {
                Expression = $"ae <= {FormatCoeff(r.MaxD)}*D"
            });
        }

        if (graph.ApVsDiameterInequalities.Count == 0 && graph.ApVsDiameterRange is not null)
        {
            var r = graph.ApVsDiameterRange;
            graph.ApVsDiameterInequalities.Add(new DiameterInequality
            {
                Expression = r.MinD == 0 ? "ap >= 0" : $"ap >= {FormatCoeff(r.MinD)}*D"
            });
            graph.ApVsDiameterInequalities.Add(new DiameterInequality
            {
                Expression = $"ap <= {FormatCoeff(r.MaxD)}*D"
            });
        }
    }

    public static void SyncPolygonsFromInequalities(ConstraintGraph graph)
    {
        if (graph.EngagementMode != EngagementMode.DiameterScaled) return;

        var plotMaxX = graph.DiameterPlotMaxMm > 0 ? graph.DiameterPlotMaxMm : DefaultPlotMaxMm;
        var aeParsed = ParseAe(graph);
        var apParsed = ParseAp(graph);

        if (aeParsed.Count > 0)
        {
            var yMax = ResolvePlotMaxYMm(aeParsed, plotMaxX, Array.Empty<double>());
            graph.EngagementAeVsDiameterPolygon = BuildPassRegionPolygon(aeParsed, plotMaxX, yMax);
        }

        if (apParsed.Count > 0)
        {
            var yMax = ResolvePlotMaxYMm(apParsed, plotMaxX, Array.Empty<double>());
            graph.EngagementApVsDiameterPolygon = BuildPassRegionPolygon(apParsed, plotMaxX, yMax);
        }
    }

    public static List<string> Validate(ConstraintGraph graph)
    {
        var errors = new List<string>();
        if (graph.EngagementMode != EngagementMode.DiameterScaled) return errors;

        ValidateList(graph.GraphNumber, "ae vs Ø", graph.AeVsDiameterInequalities, "ae", graph.DiameterPlotMaxMm, errors);
        ValidateList(graph.GraphNumber, "ap vs Ø", graph.ApVsDiameterInequalities, "ap", graph.DiameterPlotMaxMm, errors);

        if (graph.DiameterPlotMaxMm <= 0)
            errors.Add($"Graph {graph.GraphNumber}: plot Ø max must be greater than zero.");

        return errors;
    }

    private static void ValidateList(string graphNumber, string label, List<DiameterInequality> list, string variable, double plotMaxMm, List<string> errors)
    {
        var exprs = list.Select(i => i.Expression).Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        if (exprs.Count == 0)
        {
            errors.Add($"Graph {graphNumber}: {label} needs at least one inequality.");
            return;
        }

        if (!DiameterInequalityParser.TryParseAll(exprs, variable, out var parsed, out var parseErrors))
            errors.AddRange(parseErrors.Select(e => $"Graph {graphNumber}: {label}: {e}"));

        if (parsed.Count > 0 && plotMaxMm > 0)
        {
            var yMax = ResolvePlotMaxYMm(parsed, plotMaxMm, Array.Empty<double>());
            var poly = BuildPassRegionPolygon(parsed, plotMaxMm, yMax);
            if (poly.Count < 3)
                errors.Add($"Graph {graphNumber}: {label} inequalities do not define a visible pass region on the chart.");
        }
    }

    private static string FormatCoeff(double v) =>
        v == Math.Floor(v) ? v.ToString("0", System.Globalization.CultureInfo.InvariantCulture)
            : v.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    private static List<Point2D> ClipHalfPlane(IReadOnlyList<Point2D> input, double a, double b, double c)
    {
        if (input.Count == 0) return new List<Point2D>();
        var output = new List<Point2D>();

        for (var i = 0; i < input.Count; i++)
        {
            var curr = input[i];
            var prev = input[(i + input.Count - 1) % input.Count];
            var currIn = Inside(curr);
            var prevIn = Inside(prev);

            if (currIn)
            {
                if (!prevIn)
                {
                    var inter = Intersect(prev, curr);
                    if (inter is not null) output.Add(inter);
                }
                output.Add(curr);
            }
            else if (prevIn)
            {
                var inter = Intersect(prev, curr);
                if (inter is not null) output.Add(inter);
            }
        }

        return output;

        bool Inside(Point2D p) => a * p.X + b * p.Y + c >= -ClipEpsilon;

        Point2D? Intersect(Point2D p1, Point2D p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            var denom = a * dx + b * dy;
            if (Math.Abs(denom) < ClipEpsilon) return null;
            var t = -(a * p1.X + b * p1.Y + c) / denom;
            if (t < -ClipEpsilon || t > 1 + ClipEpsilon) return null;
            t = Math.Clamp(t, 0, 1);
            return new Point2D(p1.X + t * dx, p1.Y + t * dy);
        }
    }
}
