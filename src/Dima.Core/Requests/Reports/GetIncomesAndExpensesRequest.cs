namespace Dima.Core.Requests.Reports;

public class GetIncomesAndExpensesRequest : Request
{
    public int? Year { get; set; }
    public int? Month { get; set; }
}
