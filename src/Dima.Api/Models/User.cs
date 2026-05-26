using Microsoft.AspNetCore.Identity;

namespace Dima.Api.Models;

public class User : IdentityUser<long>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<IdentityRole<long>>? Roles { get; set; }
}