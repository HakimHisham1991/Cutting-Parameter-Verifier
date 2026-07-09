using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public static class CuttingDataRowValidator
{
    public static void Revalidate(CuttingDataRow row, VerificationConfig config) =>
        Revalidate(row, GetKnownMappingValues(config));

    public static void Revalidate(CuttingDataRow row, KnownMappingValues knownMapping)
    {
        row.ValidationErrors.Clear();

        void Req(string name, bool ok)
        {
            if (!ok) row.ValidationErrors.Add($"{name} is missing or invalid.");
        }

        Req("Vc (surface speed)", row.SurfaceSpeedVcMMin is > 0);
        Req("Fz (feed per tooth)", row.FeedPerToothFzMm is > 0);
        Req("ae (radial DOC)", row.RadialDocAeMm is > 0);
        Req("ap (axial DOC)", row.AxialDocApMm is > 0);
        ValidateMappingField(row, knownMapping.ProcessSpecs, row.ProcessSpecs, "Process Specs", requiredWhenKnown: false);
        ValidateMappingField(row, knownMapping.Materials, row.Material, "Material Type");
        ValidateMappingField(row, knownMapping.MillingTypes, row.MillingType, "Cutter Type");
        ValidateMappingField(row, knownMapping.ToolTypes, row.ToolType, "Tool Type (Carbide/HSS/PCD)");
        ValidateMappingField(row, knownMapping.StrategyTypes, row.StrategyType, "Machining Type (Conventional/HSM)");
        ValidateMappingField(
            row,
            knownMapping.SurfaceTypes,
            row.SurfaceType,
            "Finish Type (Finish / Controlled Roughing / Free Roughing)");

        row.IsValid = row.ValidationErrors.Count == 0;
        row.Remarks = row.IsValid ? "" : string.Join("; ", row.ValidationErrors);
    }

    public static KnownMappingValues GetKnownMappingValues(VerificationConfig config)
    {
        var rules = config.MappingRules;
        return new KnownMappingValues
        {
            ProcessSpecs = CollectDistinctMappingValues(rules.Where(r => r.UseProcessSpecs).Select(r => r.ProcessSpecs)),
            Materials = CollectDistinctMappingValues(rules.Where(r => r.UseMaterial).Select(r => r.Material)),
            SurfaceTypes = CollectDistinctMappingValues(rules.Where(r => r.UseSurfaceType).Select(r => r.SurfaceType)),
            MillingTypes = CollectDistinctMappingValues(rules.Where(r => r.UseMillingType).Select(r => r.MillingType)),
            ToolTypes = CollectDistinctMappingValues(rules.Where(r => r.UseToolType).Select(r => r.ToolType)),
            StrategyTypes = CollectDistinctMappingValues(rules.Where(r => r.UseStrategyType).Select(r => r.StrategyType)),
        };
    }

    private static HashSet<string> CollectDistinctMappingValues(IEnumerable<string> values) =>
        values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Where(v => !string.Equals(v.Trim(), MappingRule.IgnoredFieldPlaceholder, StringComparison.OrdinalIgnoreCase))
            .Select(NormalizeMappingToken)
            .ToHashSet(StringComparer.Ordinal);

    private static void ValidateMappingField(
        CuttingDataRow row,
        IReadOnlySet<string> knownValues,
        string actual,
        string columnLabel,
        bool requiredWhenKnown = true)
    {
        if (string.IsNullOrWhiteSpace(actual))
        {
            if (requiredWhenKnown || knownValues.Count > 0)
                row.ValidationErrors.Add($"{columnLabel} is missing or invalid.");
            return;
        }

        if (knownValues.Count == 0)
            return;

        if (!knownValues.Contains(NormalizeMappingToken(actual)))
            row.ValidationErrors.Add($"{columnLabel} is missing or invalid.");
    }

    private static string NormalizeMappingToken(string value) => value.Trim().ToLowerInvariant();

    public sealed class KnownMappingValues
    {
        public HashSet<string> ProcessSpecs { get; init; } = [];
        public HashSet<string> Materials { get; init; } = [];
        public HashSet<string> SurfaceTypes { get; init; } = [];
        public HashSet<string> MillingTypes { get; init; } = [];
        public HashSet<string> ToolTypes { get; init; } = [];
        public HashSet<string> StrategyTypes { get; init; } = [];
    }
}
