namespace CuttingParameterVerifier.Models;

/// <summary>Evaluation output for one imported CAM row.</summary>
public sealed class ResultRow
{
    public CuttingDataRow Source { get; init; } = null!;
    public string? GraphNumber { get; init; }
    public PassFailNa ParameterStatus { get; init; }
    public PassFailNa EngagementStatus { get; init; }
}
