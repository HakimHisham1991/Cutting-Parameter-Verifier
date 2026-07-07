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

    /// <summary>Vertices X = Ø (mm), Y = ae (mm). Lines through the origin encode ratio limits (e.g. ae ≤ 1D).</summary>
    public List<Point2D> EngagementAeVsDiameterPolygon { get; set; } = new();

    /// <summary>Vertices X = Ø (mm), Y = ap (mm). Lines through the origin encode ratio limits (e.g. 0.5D ≤ ap ≤ 1D).</summary>
    public List<Point2D> EngagementApVsDiameterPolygon { get; set; } = new();
}
