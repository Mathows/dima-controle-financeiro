using System.Net.Http.Json;
using Dima.Core.Handlers;
using Dima.Core.Requests.Account;
using Dima.Core.Responses;
using Dima.Web.Security;

namespace Dima.Web.Handlers;

public class AccountHandler(IHttpClientFactory httpClientFactory, ITokenStorage tokenStorage) : IAccountHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    private record TokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);

    public async Task<Response<string>> LoginAsync(LoginRequest request)
    {
        var result = await _client.PostAsJsonAsync("v1/identity/login", request);
        if (!result.IsSuccessStatusCode)
            return new Response<string>(null, 400, "E-mail ou senha inválidos");

        var tokens = await result.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokens is null || string.IsNullOrEmpty(tokens.AccessToken))
            return new Response<string>(null, 400, "Resposta de autenticação inválida");

        await tokenStorage.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken);
        return new Response<string>("Login realizado com sucesso!", 200, "Login realizado com sucesso!");
    }

    public async Task<Response<string>> RegisterAsync(RegisterRequest request)
    {
        var result = await _client.PostAsJsonAsync("v1/identity/account/register", request);
        if (result.IsSuccessStatusCode)
            return new Response<string>("Cadastro realizado com sucesso!", 201, "Cadastro realizado com sucesso!");

        var error = await result.Content.ReadFromJsonAsync<Response<string?>>();
        return new Response<string>(null, 400, error?.Message ?? "Não foi possível realizar o seu cadastro");
    }

    public async Task<Response<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var result = await _client.PostAsJsonAsync("v1/identity/forgotPassword", new { email = request.Email });
        return result.IsSuccessStatusCode
            ? new Response<string>("Se o e-mail existir, você receberá instruções para redefinir sua senha.", 200,
                "Se o e-mail existir, você receberá instruções para redefinir sua senha.")
            : new Response<string>(null, 400, "Não foi possível solicitar a redefinição da senha");
    }

    public async Task<Response<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var payload = new
        {
            email = request.Email,
            resetCode = request.ResetCode,
            newPassword = request.NewPassword
        };
        var result = await _client.PostAsJsonAsync("v1/identity/resetPassword", payload);
        return result.IsSuccessStatusCode
            ? new Response<string>("Senha redefinida com sucesso!", 200, "Senha redefinida com sucesso!")
            : new Response<string>(null, 400, "Não foi possível redefinir a senha. O código pode estar expirado ou inválido.");
    }

    public async Task LogoutAsync()
    {
        await tokenStorage.ClearAsync();
    }
}
