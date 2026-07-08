namespace CuttingParameterVerifier.Models;

/// <summary>One diameter-scaled bound, e.g. <c>ae &gt;= 0</c> or <c>ap &lt;= 1*D</c>.</summary>
public sealed class DiameterInequality
{
    public string Expression { get; set; } = "";
}
