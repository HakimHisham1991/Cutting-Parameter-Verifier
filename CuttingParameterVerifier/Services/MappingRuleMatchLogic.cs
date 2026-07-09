using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>Shared mapping-key comparison and overlap checks for matching rules.</summary>
public static class MappingRuleMatchLogic
{
    public static bool SameMappingKey(MappingRule a, MappingRule b) =>
        a.UseProcessSpecs == b.UseProcessSpecs &&
        a.UseMaterial == b.UseMaterial &&
        a.UseSurfaceType == b.UseSurfaceType &&
        a.UseMillingType == b.UseMillingType &&
        a.UseToolType == b.UseToolType &&
        a.UseStrategyType == b.UseStrategyType &&
        string.Equals(Norm(a.ProcessSpecs), Norm(b.ProcessSpecs), StringComparison.Ordinal) &&
        string.Equals(Norm(a.Material), Norm(b.Material), StringComparison.Ordinal) &&
        string.Equals(Norm(a.SurfaceType), Norm(b.SurfaceType), StringComparison.Ordinal) &&
        string.Equals(Norm(a.MillingType), Norm(b.MillingType), StringComparison.Ordinal) &&
        string.Equals(Norm(a.ToolType), Norm(b.ToolType), StringComparison.Ordinal) &&
        string.Equals(Norm(a.StrategyType), Norm(b.StrategyType), StringComparison.Ordinal);

    public static bool SameGraphNumber(string a, string b) =>
        string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    public static bool AreEquivalent(MappingRule a, MappingRule b) =>
        SameMappingKey(a, b) && SameGraphNumber(a.GraphNumber, b.GraphNumber);

    /// <summary>Stable id for a bundled mapping rule — used to remember intentional deletions.</summary>
    public static string BundledMappingFingerprint(MappingRule r) =>
        string.Join('\u001f',
            r.UseProcessSpecs, r.UseMaterial, r.UseSurfaceType, r.UseMillingType, r.UseToolType, r.UseStrategyType,
            Norm(r.ProcessSpecs), Norm(r.Material), Norm(r.SurfaceType), Norm(r.MillingType), Norm(r.ToolType),
            Norm(r.StrategyType), Norm(r.GraphNumber));

    private static string Norm(string s) => s.Trim().ToLowerInvariant();
}
