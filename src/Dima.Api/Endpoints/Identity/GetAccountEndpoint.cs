using System.Security.Claims;
using Dima.Api.Common.Api;
using Dima.Core.Models.Account;
using Dima.Core.Responses;
using Microsoft.AspNetCore.Identity;
using User = Dima.Api.Models.User;

namespace Dima.Api.Endpoints.Identity;

public class GetAccountEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/account/me", HandleAsync)
            .RequireAuthorization()
            .WithName("Account: GetMe")
            .WithSummary("Retorna o perfil do usuário autenticado")
            .Produces<Response<UserProfile?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        UserManager<User> userManager)
    {
        var email = principal.Identity?.Name ?? principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return TypedResults.Unauthorized();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return TypedResults.NotFound(new Response<UserProfile?>(null, 404, "Usuário não encontrado"));

        var profile = new UserProfile
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty
        };

        return TypedResults.Ok(new Response<UserProfile?>(profile));
    }
}
