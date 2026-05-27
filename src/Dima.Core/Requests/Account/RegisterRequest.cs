using System.ComponentModel.DataAnnotations;

namespace Dima.Core.Requests.Account;

public class RegisterRequest : Request
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(80, ErrorMessage = "O nome deve conter até 80 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sobrenome é obrigatório")]
    [MaxLength(80, ErrorMessage = "O sobrenome deve conter até 80 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve conter ao menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}