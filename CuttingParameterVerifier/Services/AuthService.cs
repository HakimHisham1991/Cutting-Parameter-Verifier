using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CuttingParameterVerifier.Services;

public sealed class AuthService : IAuthService
{
    private static readonly Dictionary<string, string> Users = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = "abc12345",
        ["pdc"] = "abc12345"
    };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public bool ValidateCredentials(string username, string password)
    {
        var key = username.Trim();
        return Users.TryGetValue(key, out var expected) && expected == password;
    }

    public async Task<bool> SignInAsync(string username, string password)
    {
        if (!ValidateCredentials(username, password))
            return false;

        var context = _httpContextAccessor.HttpContext;
        if (context is null)
            return false;

        var name = username.Trim();
        var claims = new List<Claim> { new(ClaimTypes.Name, name) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return true;
    }

    public Task SignOutAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        return context is null
            ? Task.CompletedTask
            : context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
