namespace CuttingParameterVerifier.Models;

/// <summary>Root JSON document for mapping rules and constraint polygons.</summary>
public sealed class VerificationConfig
{
    public List<MappingRule> MappingRules { get; set; } = new();
    public List<ConstraintGraph> Graphs { get; set; } = new();

    /// <summary>
    /// Bundled graph numbers the user intentionally removed. Merge-on-load skips these so
    /// deleted library graphs are not resurrected after Save or Reload.
    /// </summary>
    public List<string> RemovedBundledGraphNumbers { get; set; } = new();

    /// <summary>
    /// Fingerprints of bundled mapping rules the user intentionally removed. See
    /// <see cref="MappingRuleMatchLogic.BundledMappingFingerprint"/>.
    /// </summary>
    public List<string> RemovedBundledMappingFingerprints { get; set; } = new();
}
