namespace CuttingParameterVerifier.Models;

/// <summary>Diameter-scaled engagement limit expressed as a multiple of tool Ø (D).</summary>
public sealed class DiameterRatioRange
{
    /// <summary>Lower bound coefficient: value ≥ MinD × Ø.</summary>
    public double MinD { get; set; }

    /// <summary>Upper bound coefficient: value ≤ MaxD × Ø.</summary>
    public double MaxD { get; set; } = 1;

    public string Format(string variable) =>
        $"{FormatCoeff(MinD)}D ≤ {variable} ≤ {FormatCoeff(MaxD)}D";

    private static string FormatCoeff(double v) =>
        v == Math.Floor(v) ? v.ToString("0") : v.ToString("0.##");
}
