using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CuttingParameterVerifier.Services;

public sealed class ServerAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerAuthStateProvider(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }

    public void NotifyChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
