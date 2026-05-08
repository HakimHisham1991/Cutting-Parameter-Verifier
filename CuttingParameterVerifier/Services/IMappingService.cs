using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public interface IMappingService
{
    string? ResolveGraphNumber(CuttingDataRow row, VerificationConfig config);
}
