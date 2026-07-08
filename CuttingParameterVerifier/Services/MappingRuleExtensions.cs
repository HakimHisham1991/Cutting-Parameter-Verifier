using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public static class MappingRuleExtensions
{
    public static void SetFieldEnabled(MappingRule rule, MappingField field, bool enabled)
    {
        switch (field)
        {
            case MappingField.ProcessSpecs:
                rule.UseProcessSpecs = enabled;
                if (!enabled) rule.ProcessSpecs = MappingRule.IgnoredFieldPlaceholder;
                else if (IsIgnoredPlaceholder(rule.ProcessSpecs)) rule.ProcessSpecs = "";
                break;
            case MappingField.Material:
                rule.UseMaterial = enabled;
                if (!enabled) rule.Material = MappingRule.IgnoredFieldPlaceholder;
                else if (IsIgnoredPlaceholder(rule.Material)) rule.Material = "";
                break;
            case MappingField.SurfaceType:
                rule.UseSurfaceType = enabled;
                if (!enabled) rule.SurfaceType = MappingRule.IgnoredFieldPlaceholder;
                else if (IsIgnoredPlaceholder(rule.SurfaceType)) rule.SurfaceType = "";
                break;
            case MappingField.MillingType:
                rule.UseMillingType = enabled;
                if (!enabled) rule.MillingType = MappingRule.IgnoredFieldPlaceholder;
                else if (IsIgnoredPlaceholder(rule.MillingType)) rule.MillingType = "";
                break;
            case MappingField.ToolType:
                rule.UseToolType = enabled;
                if (!enabled) rule.ToolType = MappingRule.IgnoredFieldPlaceholder;
                else if (IsIgnoredPlaceholder(rule.ToolType)) rule.ToolType = "";
                break;
            case MappingField.StrategyType:
                rule.UseStrategyType = enabled;
                if (!enabled) rule.StrategyType = MappingRule.IgnoredFieldPlaceholder;
                else if (IsIgnoredPlaceholder(rule.StrategyType)) rule.StrategyType = "";
                break;
        }
    }

    public static bool IsFieldEnabled(MappingRule rule, MappingField field) => field switch
    {
        MappingField.ProcessSpecs => rule.UseProcessSpecs,
        MappingField.Material => rule.UseMaterial,
        MappingField.SurfaceType => rule.UseSurfaceType,
        MappingField.MillingType => rule.UseMillingType,
        MappingField.ToolType => rule.UseToolType,
        MappingField.StrategyType => rule.UseStrategyType,
        _ => true
    };

    public static bool IsIgnoredPlaceholder(string? value) =>
        string.Equals((value ?? "").Trim(), MappingRule.IgnoredFieldPlaceholder, StringComparison.OrdinalIgnoreCase);
}

public enum MappingField
{
    ProcessSpecs,
    Material,
    SurfaceType,
    MillingType,
    ToolType,
    StrategyType
}
