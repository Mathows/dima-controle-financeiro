using Microsoft.JSInterop;

namespace Dima.Web.Security;

public class SessionTokenStorage(IJSRuntime jsRuntime) : ITokenStorage
{
    private const string AccessTokenKey = "dima_access_token";
    private const string RefreshTokenKey = "dima_refresh_token";

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", AccessTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", RefreshTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", AccessTokenKey, accessToken);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", RefreshTokenKey, refreshToken);
    }

    public async Task ClearAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", AccessTokenKey);
            await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", RefreshTokenKey);
        }
        catch
        {
            // Storage may be inaccessible (e.g., during prerendering); ignore.
        }
    }
}
