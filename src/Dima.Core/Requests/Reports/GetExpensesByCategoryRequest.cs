namespace Dima.Core.Requests.Reports;

public class GetExpensesByCategoryRequest : Request
{
    public int? Year { get; set; }
    public int? Month { get; set; }
}
