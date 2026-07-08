namespace CuttingParameterVerifier.Services;

using CuttingParameterVerifier.Models;

public sealed class MappingService : IMappingService
{
    /// <inheritdoc />
    public IReadOnlyList<string> ResolveGraphNumbers(CuttingDataRow row, VerificationConfig config)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string>();
        foreach (var rule in config.MappingRules)
        {
            if (MatchProcessSpecs(rule, row.ProcessSpecs) &&
                MatchField(rule.UseMaterial, rule.Material, row.Material) &&
                MatchField(rule.UseSurfaceType, rule.SurfaceType, row.SurfaceType) &&
                MatchField(rule.UseMillingType, rule.MillingType, row.MillingType) &&
                MatchField(rule.UseToolType, rule.ToolType, row.ToolType) &&
                MatchField(rule.UseStrategyType, rule.StrategyType, row.StrategyType))
            {
                var g = rule.GraphNumber.Trim();
                if (g.Length == 0 || !seen.Add(g))
                    continue;
                ordered.Add(g);
            }
        }

        return ordered;
    }

    private static bool MatchProcessSpecs(MappingRule rule, string actual)
    {
        if (!rule.UseProcessSpecs) return true;
        var expected = rule.ProcessSpecs;
        if (string.IsNullOrWhiteSpace(expected) || MappingRuleExtensions.IsIgnoredPlaceholder(expected))
            return true;
        return string.Equals(Normalize(expected), Normalize(actual), StringComparison.Ordinal);
    }

    private static bool MatchField(bool useField, string expected, string actual)
    {
        if (!useField) return true;
        if (MappingRuleExtensions.IsIgnoredPlaceholder(expected)) return true;
        return string.Equals(Normalize(expected), Normalize(actual), StringComparison.Ordinal);
    }

    private static string Normalize(string s) => s.Trim().ToLowerInvariant();
}
