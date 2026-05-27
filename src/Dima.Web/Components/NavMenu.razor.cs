using System.Security.Claims;
using Microsoft.AspNetCore.Components;

namespace Dima.Web.Components;

public class NavMenuComponent : ComponentBase
{
    protected static string GetDisplayName(ClaimsPrincipal user)
    {
        var firstName = user.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = user.FindFirst(ClaimTypes.Surname)?.Value;
        var fullName = string.Join(" ", new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return string.IsNullOrWhiteSpace(fullName) ? user.Identity?.Name ?? "Conta" : fullName;
    }

    protected static string GetInitials(ClaimsPrincipal user)
    {
        var firstName = user.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = user.FindFirst(ClaimTypes.Surname)?.Value;

        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            return $"{firstName[0]}{lastName[0]}".ToUpperInvariant();

        var name = firstName ?? user.Identity?.Name ?? "?";
        return string.IsNullOrWhiteSpace(name) ? "?" : name[0].ToString().ToUpperInvariant();
    }
}
