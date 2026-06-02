using Dima.Core.Enums;

namespace Dima.Core.Requests.Transactions;

public class DeleteTransactionRequest : Request
{
    public long Id { get; set; }
    public ERecurrenceScope Scope { get; set; } = ERecurrenceScope.OnlyThis;
}
