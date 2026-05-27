using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using Dima.Api.Models;
using Dima.Core;
using Microsoft.AspNetCore.Identity;

namespace Dima.Api.Services;

public class ResendEmailSender(
    HttpClient httpClient,
    ILogger<ResendEmailSender> logger) : IEmailSender<User>
{
    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
        => SendAsync(
            email,
            "Confirme seu cadastro no Dima",
            BuildConfirmationHtml(user, confirmationLink));

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        => SendAsync(
            email,
            "Redefinição de senha - Dima",
            BuildResetHtml(user, resetLink));

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        var encodedCode = HttpUtility.UrlEncode(resetCode);
        var encodedEmail = HttpUtility.UrlEncode(email);
        var resetUrl = $"{Configuration.FrontendUrl}/redefinir-senha?email={encodedEmail}&code={encodedCode}";

        return SendAsync(
            email,
            "Redefinição de senha - Dima",
            BuildResetHtml(user, resetUrl));
    }

    private async Task SendAsync(string to, string subject, string html)
    {
        if (string.IsNullOrWhiteSpace(ApiConfiguration.ResendApiKey))
        {
            logger.LogWarning(
                "Resend API key not configured. Email to {To} with subject '{Subject}' will not be sent. HTML body:\n{Html}",
                to, subject, html);
            return;
        }

        var payload = new
        {
            from = ApiConfiguration.EmailFrom,
            to = new[] { to },
            subject,
            html
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ApiConfiguration.ResendApiKey);

        var response = await httpClient.PostAsJsonAsync("https://api.resend.com/emails", payload);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            logger.LogError(
                "Falha ao enviar e-mail via Resend para {To}. Status: {Status}. Resposta: {Body}",
                to, response.StatusCode, body);
        }
        else
        {
            logger.LogInformation("E-mail enviado via Resend para {To} — assunto: {Subject}", to, subject);
        }
    }

    private static string BuildResetHtml(User user, string resetUrl)
    {
        return $$"""
            <div style="font-family: Arial, sans-serif; max-width: 560px; margin: 0 auto; padding: 24px;">
              <h1 style="color: #1A1A1A;">Olá!</h1>
              <p>Recebemos um pedido para redefinir a senha da sua conta no Dima.</p>
              <p>Clique no botão abaixo para criar uma nova senha:</p>
              <p style="margin: 32px 0;">
                <a href="{{resetUrl}}" style="background-color: #1EFA2D; color: #000; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;">
                  Redefinir senha
                </a>
              </p>
              <p style="font-size: 12px; color: #666;">
                Se o botão não funcionar, copie e cole este link no navegador:<br>
                <a href="{{resetUrl}}">{{resetUrl}}</a>
              </p>
              <hr style="margin: 32px 0; border: none; border-top: 1px solid #eee;">
              <p style="font-size: 11px; color: #999;">
                Se você não solicitou esta redefinição, ignore este e-mail. Sua senha continua a mesma.
              </p>
            </div>
            """;
    }

    private static string BuildConfirmationHtml(User user, string confirmationLink)
    {
        return $$"""
            <div style="font-family: Arial, sans-serif; max-width: 560px; margin: 0 auto; padding: 24px;">
              <h1 style="color: #1A1A1A;">Olá!</h1>
              <p>Bem-vindo ao Dima. Para ativar sua conta, clique no botão abaixo:</p>
              <p style="margin: 32px 0;">
                <a href="{{confirmationLink}}" style="background-color: #1EFA2D; color: #000; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;">
                  Confirmar conta
                </a>
              </p>
            </div>
            """;
    }
}
