namespace CuttingParameterVerifier.Models;

/// <summary>One CAM-exported operation row after Excel import.</summary>
public sealed class CuttingDataRow
{
    public int? No { get; set; }
    public string OperationName { get; set; } = "";
    public string ToolName { get; set; } = "";
    public double? DiameterMm { get; set; }
    public int? NumberOfTeethZ { get; set; }
    public double? ToolSpeedNRpm { get; set; }
    public double? FeedRateVfMmMin { get; set; }
    public double? SurfaceSpeedVcMMin { get; set; }
    public double? FeedPerToothFzMm { get; set; }
    public double? RadialDocAeMm { get; set; }
    public double? AxialDocApMm { get; set; }
    public string Material { get; set; } = "";
    public string SurfaceType { get; set; } = "";
    public string MillingType { get; set; } = "";
    public string ToolType { get; set; } = "";
    public string StrategyType { get; set; } = "";

    /// <summary>Human-readable issues for this row (invalid import / missing required fields).</summary>
    public string Remarks { get; set; } = "";

    public bool IsValid { get; set; } = true;
    public List<string> ValidationErrors { get; } = new();
}
