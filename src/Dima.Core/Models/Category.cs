using Dima.Core.Enums;

namespace Dima.Core.Models;

public class Category
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ETransactionType Type { get; set; } = ETransactionType.Withdraw;
    public string UserId { get; set; } = string.Empty;
}
