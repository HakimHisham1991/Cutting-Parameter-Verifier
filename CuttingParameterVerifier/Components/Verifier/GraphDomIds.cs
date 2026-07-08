namespace CuttingParameterVerifier.Components.Verifier;

internal static class GraphDomIds
{
    public const string SettingsPreviewPrefix = "settings-prev-";

    public static string Cutting(string graphNumber, string? prefix = null) => Prefix(prefix) + "cut-" + Sanitize(graphNumber);
    public static string Engagement(string graphNumber, string? prefix = null) => Prefix(prefix) + "eng-" + Sanitize(graphNumber);
    public static string EngagementAeVsDiameter(string graphNumber, string? prefix = null) => Prefix(prefix) + "eng-ae-dia-" + Sanitize(graphNumber);
    public static string EngagementApVsDiameter(string graphNumber, string? prefix = null) => Prefix(prefix) + "eng-ap-dia-" + Sanitize(graphNumber);
    public static string FigureAnchor(string graphNumber, string? prefix = null) => Prefix(prefix) + "fig-" + Sanitize(graphNumber);

    private static string Prefix(string? prefix) => string.IsNullOrEmpty(prefix) ? "" : prefix;

    private static string Sanitize(string graphNumber)
    {
        var chars = graphNumber.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (char.IsLetterOrDigit(c) || c is '-' or '_')
                continue;
            chars[i] = '_';
        }

        return new string(chars);
    }
}
