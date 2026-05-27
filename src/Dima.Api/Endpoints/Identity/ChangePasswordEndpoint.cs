using System.Security.Claims;
using Dima.Api.Common.Api;
using Dima.Api.Models;
using Dima.Core.Requests.Account;
using Dima.Core.Responses;
using Microsoft.AspNetCore.Identity;

namespace Dima.Api.Endpoints.Identity;

public class ChangePasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/account/change-password", HandleAsync)
            .RequireAuthorization()
            .WithName("Account: ChangePassword")
            .WithSummary("Altera a senha do usuário autenticado")
            .Produces<Response<string?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        UserManager<User> userManager,
        ChangePasswordRequest request)
    {
        var email = principal.Identity?.Name ?? principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return TypedResults.Unauthorized();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return TypedResults.NotFound(new Response<string?>(null, 404, "Usuário não encontrado"));

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return TypedResults.BadRequest(new Response<string?>(null, 400, message));
        }

        return TypedResults.Ok(new Response<string?>("ok", 200, "Senha alterada com sucesso"));
    }
}
