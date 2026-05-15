using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class MappingService : IMappingService
{
    /// <inheritdoc />
    public IReadOnlyList<string> ResolveGraphNumbers(CuttingDataRow row, VerificationConfig config)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string>();
        foreach (var rule in config.MappingRules)
        {
            if (Match(rule.Material, row.Material) &&
                Match(rule.SurfaceType, row.SurfaceType) &&
                Match(rule.MillingType, row.MillingType) &&
                Match(rule.ToolType, row.ToolType) &&
                Match(rule.StrategyType, row.StrategyType))
            {
                var g = rule.GraphNumber.Trim();
                if (g.Length == 0 || !seen.Add(g))
                    continue;
                ordered.Add(g);
            }
        }

        return ordered;
    }

    private static bool Match(string expected, string actual) =>
        string.Equals(Normalize(expected), Normalize(actual), StringComparison.Ordinal);

    private static string Normalize(string s) => s.Trim().ToLowerInvariant();
}
