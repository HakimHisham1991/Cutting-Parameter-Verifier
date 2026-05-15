using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public interface IMappingService
{
    /// <summary>All graph numbers from mapping rows that match this operation, in configured order (no duplicates).</summary>
    IReadOnlyList<string> ResolveGraphNumbers(CuttingDataRow row, VerificationConfig config);
}
