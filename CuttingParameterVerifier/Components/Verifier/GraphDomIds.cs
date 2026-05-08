namespace CuttingParameterVerifier.Components.Verifier;

internal static class GraphDomIds
{
    public static string Cutting(string graphNumber) => "cut-" + Sanitize(graphNumber);
    public static string Engagement(string graphNumber) => "eng-" + Sanitize(graphNumber);
    public static string FigureAnchor(string graphNumber) => "fig-" + Sanitize(graphNumber);

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
