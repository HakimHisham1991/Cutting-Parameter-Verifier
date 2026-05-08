using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public interface IEvaluationService
{
    IReadOnlyList<ResultRow> Evaluate(IReadOnlyList<CuttingDataRow> rows, VerificationConfig config);
}
