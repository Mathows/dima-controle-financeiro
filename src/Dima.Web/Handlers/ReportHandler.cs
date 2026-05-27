using System.Net.Http.Json;
using Dima.Core.Handlers;
using Dima.Core.Models.Reports;
using Dima.Core.Requests.Reports;
using Dima.Core.Responses;

namespace Dima.Web.Handlers;

public class ReportHandler(IHttpClientFactory httpClientFactory) : IReportHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<Response<List<IncomesAndExpenses>?>> GetIncomesAndExpensesReportAsync(
        GetIncomesAndExpensesRequest request)
    {
        var url = $"v1/reports/incomes-expenses?year={request.Year}&month={request.Month}";
        return await _client.GetFromJsonAsync<Response<List<IncomesAndExpenses>?>>(url)
               ?? new Response<List<IncomesAndExpenses>?>(null, 400, "Não foi possível obter os dados");
    }

    public async Task<Response<List<IncomesByCategory>?>> GetIncomesByCategoryReportAsync(
        GetIncomesByCategoryRequest request)
    {
        var url = $"v1/reports/incomes?year={request.Year}&month={request.Month}";
        return await _client.GetFromJsonAsync<Response<List<IncomesByCategory>?>>(url)
               ?? new Response<List<IncomesByCategory>?>(null, 400, "Não foi possível obter os dados");
    }

    public async Task<Response<List<ExpensesByCategory>?>> GetExpensesByCategoryReportAsync(
        GetExpensesByCategoryRequest request)
    {
        var url = $"v1/reports/expenses?year={request.Year}&month={request.Month}";
        return await _client.GetFromJsonAsync<Response<List<ExpensesByCategory>?>>(url)
               ?? new Response<List<ExpensesByCategory>?>(null, 400, "Não foi possível obter os dados");
    }

    public async Task<Response<FinancialSummary?>> GetFinancialSummaryReportAsync(GetFinancialSummaryRequest request)
    {
        var url = $"v1/reports/summary?year={request.Year}&month={request.Month}";
        return await _client.GetFromJsonAsync<Response<FinancialSummary?>>(url)
               ?? new Response<FinancialSummary?>(null, 400, "Não foi possível obter os dados");
    }
}
