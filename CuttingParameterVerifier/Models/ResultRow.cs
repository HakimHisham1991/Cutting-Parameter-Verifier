

namespace CuttingParameterVerifier.Models;

/// <summary>Evaluation output for one imported CAM row.</summary>
public sealed class ResultRow
{
    public CuttingDataRow Source { get; init; } = null!;

    /// <summary>All graph identifiers from mapping rules that match this row (e.g. two rows in Settings → two IDs).</summary>
    public IReadOnlyList<string>? MatchedGraphNumbers { get; init; }

    /// <summary>Comma-separated figures for UI and export (<see cref="MatchedGraphNumbers"/>).</summary>
    public string? FigureNumbersDisplay =>
        MatchedGraphNumbers is { Count: > 0 } ? string.Join(", ", MatchedGraphNumbers) : null;

    public PassFailNa ParameterStatus { get; init; }
    public PassFailNa EngagementStatus { get; init; }

    /// <summary>ae vs Ø check (Pass/Fail in diameter-scaled mode; N/A in ap × ae mode).</summary>
    public PassFailNa AeCheckStatus { get; init; }

    /// <summary>ap vs Ø check (Pass/Fail in diameter-scaled mode; N/A in ap × ae mode).</summary>
    public PassFailNa ApCheckStatus { get; init; }

    /// <summary>Per matched figure: cutting (Vc–Fz) check, same order as <see cref="MatchedGraphNumbers"/>.</summary>
    public IReadOnlyList<PassFailNa>? ParameterStatusesPerGraph { get; init; }

    /// <summary>Per matched figure: engagement check, same order as <see cref="MatchedGraphNumbers"/>.</summary>
    public IReadOnlyList<PassFailNa>? EngagementStatusesPerGraph { get; init; }

    /// <summary>Per matched figure: ae vs Ø check, same order as <see cref="MatchedGraphNumbers"/>.</summary>
    public IReadOnlyList<PassFailNa>? AeCheckStatusesPerGraph { get; init; }

    /// <summary>Per matched figure: ap vs Ø check, same order as <see cref="MatchedGraphNumbers"/>.</summary>
    public IReadOnlyList<PassFailNa>? ApCheckStatusesPerGraph { get; init; }

    /// <summary>Primary graph tab target (first matched figure).</summary>
    public string? PrimaryGraphNumber =>
        MatchedGraphNumbers is { Count: > 0 } ? MatchedGraphNumbers[0].Trim() : null;
}
