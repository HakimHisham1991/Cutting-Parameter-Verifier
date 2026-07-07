namespace CuttingParameterVerifier.Models;

/// <summary>How engagement constraints are defined and evaluated for a graph.</summary>
public enum EngagementMode
{
    /// <summary>Single polygon in ap × ae (mm). Default — first customer specs.</summary>
    ApAe,

    /// <summary>Two polygons: ae vs Ø and ap vs Ø (mm). Ratio limits like 1D scale with diameter.</summary>
    DiameterScaled
}
