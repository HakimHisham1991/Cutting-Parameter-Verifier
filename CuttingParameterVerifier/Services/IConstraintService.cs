using CuttingParameterVerifier.Models;

namespace CuttingParameterVerifier.Services;

public interface IConstraintService
{
    event Action? ConfigurationChanged;

    VerificationConfig Load();

    void Save(VerificationConfig config);

    ConstraintGraph? FindGraph(VerificationConfig config, string graphNumber);
}
