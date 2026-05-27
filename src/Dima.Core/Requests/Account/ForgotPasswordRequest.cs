using System.ComponentModel.DataAnnotations;

namespace Dima.Core.Requests.Account;

public class ForgotPasswordRequest : Request
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
}
