using System.Text;
using System.Text.Json;

namespace Dima.Mobile.Security;

public interface IUserContext
{
    Task<(string DisplayName, string Initials, string? Email)> GetAsync();
}

public class UserContext(ITokenStorage tokenStorage) : IUserContext
{
    public async Task<(string DisplayName, string Initials, string? Email)> GetAsync()
    {
        var token = await tokenStorage.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return ("Conta", "?", null);

        var claims = DecodeJwtPayload(token);

        var firstName = TryGet(claims, "given_name")
                        ?? TryGet(claims, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
        var lastName = TryGet(claims, "family_name")
                       ?? TryGet(claims, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
        var email = TryGet(claims, "email")
                    ?? TryGet(claims, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                    ?? TryGet(claims, "name");

        var displayName = string.Join(" ",
            new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = email ?? "Conta";

        var initials = GetInitials(firstName, lastName, email);

        return (displayName, initials, email);
    }

    private static string? TryGet(IDictionary<string, string> claims, string key)
        => claims.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static string GetInitials(string? firstName, string? lastName, string? email)
    {
        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            return $"{firstName[0]}{lastName[0]}".ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(firstName))
            return firstName[0].ToString().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(email))
            return email[0].ToString().ToUpperInvariant();

        return "?";
    }

    private static Dictionary<string, string> DecodeJwtPayload(string token)
    {
        var result = new Dictionary<string, string>();
        var parts = token.Split('.');
        if (parts.Length < 2) return result;

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            using var document = JsonDocument.Parse(json);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                    result[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }
        catch
        {
            // token inválido ou malformado — retorna dicionário vazio
        }

        return result;
    }
}
