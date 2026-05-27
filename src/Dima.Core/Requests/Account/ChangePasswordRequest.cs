using System.ComponentModel.DataAnnotations;

namespace Dima.Core.Requests.Account;

public class ChangePasswordRequest : Request
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A nova senha deve conter ao menos 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}
