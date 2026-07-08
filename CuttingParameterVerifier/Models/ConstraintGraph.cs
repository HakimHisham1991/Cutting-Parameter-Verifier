using CuttingParameterVerifier.Services;

namespace CuttingParameterVerifier.Models;

/// <summary>One logical graph bundle: cutting (Vc vs Fz) and engagement constraint polygon(s).</summary>
public sealed class ConstraintGraph
{
    public string GraphNumber { get; set; } = "";

    public List<Point2D> CuttingPolygon { get; set; } = new();

    /// <summary>ap × ae (mm) polygon — used when <see cref="EngagementMode"/> is <see cref="EngagementMode.ApAe"/>.</summary>
    public List<Point2D> EngagementPolygon { get; set; } = new();

    /// <summary>When <see cref="EngagementMode.DiameterScaled"/>, engagement uses separate ae vs Ø and ap vs Ø polygons.</summary>
    public EngagementMode EngagementMode { get; set; } = EngagementMode.ApAe;

    /// <summary>ae vs Ø range in D-multiples (e.g. 0D ≤ ae ≤ 1D). Primary authoring source when diameter-scaled.</summary>
    public DiameterRatioRange? AeVsDiameterRange { get; set; }

    /// <summary>ap vs Ø range in D-multiples (e.g. 0.5D ≤ ap ≤ 1D). Primary authoring source when diameter-scaled.</summary>
    public DiameterRatioRange? ApVsDiameterRange { get; set; }

    /// <summary>Ø axis extent (mm) for chart boundary lines when diameter-scaled.</summary>
    public double DiameterPlotMaxMm { get; set; } = DiameterRangeService.DefaultPlotMaxMm;

    /// <summary>Compiled band polygon X = Ø (mm), Y = ae (mm). Synced from <see cref="AeVsDiameterRange"/> on save.</summary>
    public List<Point2D> EngagementAeVsDiameterPolygon { get; set; } = new();

    /// <summary>Compiled band polygon X = Ø (mm), Y = ap (mm). Synced from <see cref="ApVsDiameterRange"/> on save.</summary>
    public List<Point2D> EngagementApVsDiameterPolygon { get; set; } = new();
}
