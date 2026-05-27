using System.ComponentModel.DataAnnotations;

namespace Dima.Core.Requests.Account;

public class UpdateProfileRequest : Request
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(80, ErrorMessage = "O nome deve conter até 80 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sobrenome é obrigatório")]
    [MaxLength(80, ErrorMessage = "O sobrenome deve conter até 80 caracteres")]
    public string LastName { get; set; } = string.Empty;
}
