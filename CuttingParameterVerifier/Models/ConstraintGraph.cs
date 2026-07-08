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

    /// <summary>ae vs Ø inequalities (e.g. ae &gt;= 0, ae &lt;= 1*D).</summary>
    public List<DiameterInequality> AeVsDiameterInequalities { get; set; } = new();

    /// <summary>ap vs Ø inequalities (e.g. ap &gt;= 0.5*D, ap &lt;= 1*D).</summary>
    public List<DiameterInequality> ApVsDiameterInequalities { get; set; } = new();

    /// <summary>Legacy min/max D range — migrated to inequalities on load.</summary>
    public DiameterRatioRange? AeVsDiameterRange { get; set; }

    /// <summary>Legacy min/max D range — migrated to inequalities on load.</summary>
    public DiameterRatioRange? ApVsDiameterRange { get; set; }

    /// <summary>Ø axis extent (mm) for chart boundary lines when diameter-scaled.</summary>
    public double DiameterPlotMaxMm { get; set; } = DiameterInequalityService.DefaultPlotMaxMm;

    /// <summary>Compiled pass-region polygon X = Ø (mm), Y = ae (mm). Synced from inequalities on save.</summary>
    public List<Point2D> EngagementAeVsDiameterPolygon { get; set; } = new();

    /// <summary>Compiled pass-region polygon X = Ø (mm), Y = ap (mm). Synced from inequalities on save.</summary>
    public List<Point2D> EngagementApVsDiameterPolygon { get; set; } = new();
}
