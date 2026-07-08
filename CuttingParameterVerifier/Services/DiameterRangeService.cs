using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>Evaluates and renders diameter-scaled engagement ranges (e.g. 0D ≤ ae ≤ 1D).</summary>
public static class DiameterRangeService
{
    public const double DefaultPlotMaxMm = 50;

    public static PassFailNa Evaluate(double valueMm, double diameterMm, DiameterRatioRange range)
    {
        if (diameterMm <= 0) return PassFailNa.Na;
        var min = range.MinD * diameterMm;
        var max = range.MaxD * diameterMm;
        return valueMm >= min && valueMm <= max ? PassFailNa.Pass : PassFailNa.Fail;
    }

    /// <summary>Band polygon for chart fill: X = Ø (mm), Y = ae or ap (mm).</summary>
    public static List<Point2D> BuildBandPolygon(DiameterRatioRange range, double diameterMaxMm)
    {
        if (diameterMaxMm <= 0) diameterMaxMm = DefaultPlotMaxMm;
        var xMax = diameterMaxMm;
        var yMinAtMax = range.MinD * xMax;
        var yMaxAtMax = range.MaxD * xMax;

        return new List<Point2D>
        {
            new(0, 0),
            new(xMax, yMinAtMax),
            new(xMax, yMaxAtMax),
            new(0, 0)
        };
    }

    public static void EnsureRanges(ConstraintGraph graph)
    {
        if (graph.EngagementMode != EngagementMode.DiameterScaled) return;

        graph.AeVsDiameterRange ??= new DiameterRatioRange { MinD = 0, MaxD = 1 };
        graph.ApVsDiameterRange ??= new DiameterRatioRange { MinD = 0.5, MaxD = 1 };

        if (graph.DiameterPlotMaxMm <= 0)
            graph.DiameterPlotMaxMm = DefaultPlotMaxMm;

        SyncPolygonsFromRanges(graph);
    }

    public static void SyncPolygonsFromRanges(ConstraintGraph graph)
    {
        if (graph.EngagementMode != EngagementMode.DiameterScaled) return;
        if (graph.AeVsDiameterRange is null || graph.ApVsDiameterRange is null) return;

        var plotMax = graph.DiameterPlotMaxMm > 0 ? graph.DiameterPlotMaxMm : DefaultPlotMaxMm;
        graph.EngagementAeVsDiameterPolygon = BuildBandPolygon(graph.AeVsDiameterRange, plotMax);
        graph.EngagementApVsDiameterPolygon = BuildBandPolygon(graph.ApVsDiameterRange, plotMax);
    }

    public static List<string> Validate(ConstraintGraph graph)
    {
        var errors = new List<string>();
        if (graph.EngagementMode != EngagementMode.DiameterScaled) return errors;

        ValidateRange(graph.GraphNumber, "ae vs Ø", graph.AeVsDiameterRange, errors);
        ValidateRange(graph.GraphNumber, "ap vs Ø", graph.ApVsDiameterRange, errors);

        if (graph.DiameterPlotMaxMm <= 0)
            errors.Add($"Graph {graph.GraphNumber}: plot Ø max must be greater than zero.");

        return errors;
    }

    private static void ValidateRange(string graphNumber, string label, DiameterRatioRange? range, List<string> errors)
    {
        if (range is null)
        {
            errors.Add($"Graph {graphNumber}: {label} range is required.");
            return;
        }

        if (range.MinD < 0)
            errors.Add($"Graph {graphNumber}: {label} min must be ≥ 0.");
        if (range.MaxD < 0)
            errors.Add($"Graph {graphNumber}: {label} max must be ≥ 0.");
        if (range.MinD > range.MaxD)
            errors.Add($"Graph {graphNumber}: {label} min must be ≤ max.");
    }

    /// <summary>Plot extent: configured max or data-driven, whichever is larger.</summary>
    public static double ResolvePlotMaxMm(ConstraintGraph graph, IEnumerable<double> dataDiametersMm)
    {
        var configured = graph.DiameterPlotMaxMm > 0 ? graph.DiameterPlotMaxMm : DefaultPlotMaxMm;
        var dataMax = dataDiametersMm.Where(d => d > 0).DefaultIfEmpty(0).Max();
        if (dataMax <= 0) return configured;
        return Math.Max(configured, Math.Ceiling(dataMax * 1.1 * 100) / 100);
    }
}
