using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public interface IExcelService
{
    Task<IReadOnlyList<CuttingDataRow>> ReadAsync(Stream xlsxStream, CancellationToken cancellationToken = default);
    Task WriteResultsAsync(Stream outputStream, IReadOnlyList<ResultRow> results, CancellationToken cancellationToken = default);
}
