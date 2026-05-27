using Dima.Core.Models.Account;
using Dima.Core.Requests.Account;
using Dima.Core.Responses;

namespace Dima.Core.Handlers;

public interface IAccountHandler
{
    Task<Response<string>> LoginAsync(LoginRequest request);
    Task<Response<string>> RegisterAsync(RegisterRequest request);
    Task<Response<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Response<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Response<UserProfile?>> GetProfileAsync();
    Task<Response<UserProfile?>> UpdateProfileAsync(UpdateProfileRequest request);
    Task<Response<string>> ChangePasswordAsync(ChangePasswordRequest request);
    Task LogoutAsync();
}