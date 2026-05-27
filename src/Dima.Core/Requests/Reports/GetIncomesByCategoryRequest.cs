namespace Dima.Core.Requests.Reports;

public class GetIncomesByCategoryRequest : Request
{
    public int? Year { get; set; }
    public int? Month { get; set; }
}
