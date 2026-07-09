using System.Globalization;
using System.Text.RegularExpressions;

namespace CuttingParameterVerifier.Services;

/// <summary>Parsing and validation for in-progress decimal text fields (e.g. ".2" while typing).</summary>
public static partial class PartialDoubleInput
{
    [GeneratedRegex(@"^-?\d*\.?\d*$")]
    private static partial Regex PartialPattern();

    public static bool IsAllowedWhileTyping(string? text) =>
        string.IsNullOrEmpty(text) || PartialPattern().IsMatch(text);

    public static bool TryParseOnCommit(string? text, out double? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
            return true;

        var trimmed = text.Trim();
        if (trimmed is "." or "-.")
            return false;

        var normalized = trimmed.StartsWith('.') ? "0" + trimmed
            : trimmed.StartsWith("-.") ? "-0" + trimmed[1..]
            : trimmed;

        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            return false;

        value = parsed;
        return true;
    }
}
