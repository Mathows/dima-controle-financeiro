using System.Security.Claims;
using Dima.Api.Common.Api;
using Dima.Api.Models;
using Dima.Core.Requests.Account;
using Dima.Core.Responses;
using Microsoft.AspNetCore.Identity;

namespace Dima.Api.Endpoints.Identity;

public class CreateAccountEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app
            .MapPost("/account/register", HandleAsync)
            .AllowAnonymous()
            .WithName("Account: Register")
            .WithSummary("Cria uma nova conta de usuário")
            .Produces<Response<string?>>();

    private static async Task<IResult> HandleAsync(
        UserManager<User> userManager,
        RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return TypedResults.BadRequest(new Response<string?>(null, 400, "E-mail é obrigatório"));

        if (string.IsNullOrWhiteSpace(request.Password))
            return TypedResults.BadRequest(new Response<string?>(null, 400, "Senha é obrigatória"));

        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return TypedResults.BadRequest(new Response<string?>(null, 400, "E-mail já cadastrado"));

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return TypedResults.BadRequest(new Response<string?>(null, 400, message));
        }

        await userManager.AddClaimsAsync(user,
        [
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        ]);

        return TypedResults.Created(
            $"/v1/identity/account/{user.Id}",
            new Response<string?>(user.Email, 201, "Cadastro realizado com sucesso"));
    }
}
