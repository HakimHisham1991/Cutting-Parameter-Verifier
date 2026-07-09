namespace CuttingParameterVerifier.Services;

public interface IAuthService
{
    bool ValidateCredentials(string username, string password);

    Task<bool> SignInAsync(string username, string password);

    Task SignOutAsync();
}
