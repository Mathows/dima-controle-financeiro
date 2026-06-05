using System.Net.Http.Json;
using System.Security.Claims;
using Dima.Core.Models.Account;
using Dima.Core.Responses;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dima.Web.Security;

public class CookieAuthenticationStateProvider(IHttpClientFactory clientFactory) :
    AuthenticationStateProvider,
    ICookieAuthenticationStateProvider
{
    private bool _isAuthenticated = false;
    private readonly HttpClient _client = clientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return _isAuthenticated;
    }

    public void NotifyAuthenticationStateChanged()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _isAuthenticated = false;
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var profile = await GetProfile();
        if (profile is null || string.IsNullOrWhiteSpace(profile.Email))
            return new AuthenticationState(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, profile.Email),
            new(ClaimTypes.Email, profile.Email)
        };

        if (!string.IsNullOrWhiteSpace(profile.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, profile.FirstName));
        if (!string.IsNullOrWhiteSpace(profile.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, profile.LastName));

        var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
        user = new ClaimsPrincipal(id);

        _isAuthenticated = true;
        return new AuthenticationState(user);
    }

    private async Task<UserProfile?> GetProfile()
    {
        try
        {
            var response = await _client.GetFromJsonAsync<Response<UserProfile?>>("v1/identity/account/me");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }
}