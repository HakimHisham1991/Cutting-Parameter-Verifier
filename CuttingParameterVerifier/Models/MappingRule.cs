namespace CuttingParameterVerifier.Models;

/// <summary>Maps a six-dimensional CAM context key to a constraint graph identifier.</summary>
public sealed class MappingRule
{
    public string ProcessSpecs { get; set; } = "";
    public string Material { get; set; } = "";
    public string SurfaceType { get; set; } = "";
    public string MillingType { get; set; } = "";
    public string ToolType { get; set; } = "";
    public string StrategyType { get; set; } = "";
    public string GraphNumber { get; set; } = "";
}
