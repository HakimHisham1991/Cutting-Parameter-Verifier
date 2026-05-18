using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

/// <summary>In-memory imported rows and latest evaluation snapshot for the active Blazor circuit.</summary>
public sealed class CuttingSessionState : IDisposable
{
    private readonly IEvaluationService _evaluation;
    private readonly IConstraintService _constraints;

    public CuttingSessionState(IEvaluationService evaluation, IConstraintService constraints)
    {
        _evaluation = evaluation;
        _constraints = constraints;
        _constraints.ConfigurationChanged += OnConfigurationChanged;
    }

    public IReadOnlyList<CuttingDataRow> ImportedRows { get; private set; } = Array.Empty<CuttingDataRow>();
    public IReadOnlyList<ResultRow> Results { get; private set; } = Array.Empty<ResultRow>();
    public IReadOnlyList<string> LastImportNotes { get; private set; } = Array.Empty<string>();
    public string? SelectedGraphNumber { get; private set; }

    /// <summary>Set when the user requests scrolling/focus on the figure from the results table (even if that tab is already selected).</summary>
    private bool _figureFocusRequested;

    public event Action? Changed;

    public void SetImported(IReadOnlyList<CuttingDataRow> rows, IReadOnlyList<string>? notes = null)
    {
        ImportedRows = rows.ToList();
        LastImportNotes = notes?.ToList() ?? new List<string>();
        Recompute();
    }

    public void Recompute()
    {
        var cfg = _constraints.Load();
        Results = _evaluation.Evaluate(ImportedRows, cfg).ToList();
        Changed?.Invoke();
    }

    public void SetSelectedGraphNumber(string? graphNumber)
    {
        var next = string.IsNullOrWhiteSpace(graphNumber) ? null : graphNumber.Trim();
        if (string.Equals(SelectedGraphNumber, next, StringComparison.OrdinalIgnoreCase))
            return;

        SelectedGraphNumber = next;
        Changed?.Invoke();
    }

    /// <summary>Selects a constraint figure tab and signals the gallery to scroll it into view (used from the results table).</summary>
    public void SelectAndFocusFigure(string? graphNumber)
    {
        if (string.IsNullOrWhiteSpace(graphNumber))
            return;

        var next = graphNumber.Trim();
        if (!string.Equals(SelectedGraphNumber, next, StringComparison.OrdinalIgnoreCase))
            SelectedGraphNumber = next;

        _figureFocusRequested = true;
        Changed?.Invoke();
    }

    /// <summary>Consumed by the graph gallery after a session change.</summary>
    public bool ConsumeFigureFocusRequest()
    {
        var v = _figureFocusRequested;
        _figureFocusRequested = false;
        return v;
    }

    private void OnConfigurationChanged() => Recompute();

    public void Dispose()
    {
        _constraints.ConfigurationChanged -= OnConfigurationChanged;
    }
}
