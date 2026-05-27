using System.ComponentModel.DataAnnotations;

namespace Dima.Core.Requests.Account;

public class ResetPasswordRequest : Request
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Código de redefinição é obrigatório")]
    public string ResetCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve conter ao menos 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}
