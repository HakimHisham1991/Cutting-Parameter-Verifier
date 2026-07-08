namespace CuttingParameterVerifier.Models;

/// <summary>Maps a six-dimensional CAM context key to a constraint graph identifier.</summary>
public sealed class MappingRule
{
    public const string IgnoredFieldPlaceholder = "N/A";

    public string ProcessSpecs { get; set; } = "";
    public string Material { get; set; } = "";
    public string SurfaceType { get; set; } = "";
    public string MillingType { get; set; } = "";
    public string ToolType { get; set; } = "";
    public string StrategyType { get; set; } = "";
    public string GraphNumber { get; set; } = "";

    /// <summary>When false, Process Specs is ignored for matching (wildcard).</summary>
    public bool UseProcessSpecs { get; set; } = true;

    public bool UseMaterial { get; set; } = true;
    public bool UseSurfaceType { get; set; } = true;
    public bool UseMillingType { get; set; } = true;
    public bool UseToolType { get; set; } = true;
    public bool UseStrategyType { get; set; } = true;
}
