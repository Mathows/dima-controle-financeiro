namespace Dima.Core.Requests.Reports;

public class GetFinancialSummaryRequest : Request
{
    public int? Year { get; set; }
    public int? Month { get; set; }
}
