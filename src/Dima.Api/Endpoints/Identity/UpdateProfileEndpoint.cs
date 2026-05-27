using System.Security.Claims;
using Dima.Api.Common.Api;
using Dima.Core.Models.Account;
using Dima.Core.Requests.Account;
using Dima.Core.Responses;
using Microsoft.AspNetCore.Identity;
using User = Dima.Api.Models.User;

namespace Dima.Api.Endpoints.Identity;

public class UpdateProfileEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/account/profile", HandleAsync)
            .RequireAuthorization()
            .WithName("Account: UpdateProfile")
            .WithSummary("Atualiza nome e sobrenome do usuário autenticado")
            .Produces<Response<UserProfile?>>();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        UserManager<User> userManager,
        UpdateProfileRequest request)
    {
        var email = principal.Identity?.Name ?? principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return TypedResults.Unauthorized();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return TypedResults.NotFound(new Response<UserProfile?>(null, 404, "Usuário não encontrado"));

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var message = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            return TypedResults.BadRequest(new Response<UserProfile?>(null, 400, message));
        }

        var existingClaims = await userManager.GetClaimsAsync(user);
        var givenName = existingClaims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
        if (givenName is not null)
            await userManager.RemoveClaimAsync(user, givenName);
        var surname = existingClaims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
        if (surname is not null)
            await userManager.RemoveClaimAsync(user, surname);

        await userManager.AddClaimsAsync(user,
        [
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        ]);

        var profile = new UserProfile
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty
        };

        return TypedResults.Ok(new Response<UserProfile?>(profile, 200, "Perfil atualizado com sucesso"));
    }
}
