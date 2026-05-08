namespace CuttingParameterVerifier.Models;

/// <summary>One logical graph bundle: cutting (Vc vs Fz) and engagement polygons (stored vertex X = ae, Y = ap; charts plot ap horizontal, ae vertical).</summary>
public sealed class ConstraintGraph
{
    public string GraphNumber { get; set; } = "";
    public List<Point2D> CuttingPolygon { get; set; } = new();
    public List<Point2D> EngagementPolygon { get; set; } = new();
}
