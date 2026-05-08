namespace CuttingParameterVerifier.Models;

/// <summary>Root JSON document for mapping rules and constraint polygons.</summary>
public sealed class VerificationConfig
{
    public List<MappingRule> MappingRules { get; set; } = new();
    public List<ConstraintGraph> Graphs { get; set; } = new();
}
