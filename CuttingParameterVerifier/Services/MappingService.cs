using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public sealed class MappingService : IMappingService
{
    public string? ResolveGraphNumber(CuttingDataRow row, VerificationConfig config)
    {
        foreach (var rule in config.MappingRules)
        {
            if (Match(rule.Material, row.Material) &&
                Match(rule.SurfaceType, row.SurfaceType) &&
                Match(rule.MillingType, row.MillingType) &&
                Match(rule.ToolType, row.ToolType) &&
                Match(rule.StrategyType, row.StrategyType))
            {
                return rule.GraphNumber.Trim();
            }
        }

        return null;
    }

    private static bool Match(string expected, string actual) =>
        string.Equals(Normalize(expected), Normalize(actual), StringComparison.Ordinal);

    private static string Normalize(string s) => s.Trim().ToLowerInvariant();
}
