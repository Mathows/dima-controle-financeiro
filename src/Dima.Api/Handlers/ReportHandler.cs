using Dima.Api.Data;
using Dima.Core.Enums;
using Dima.Core.Handlers;
using Dima.Core.Models.Reports;
using Dima.Core.Requests.Reports;
using Dima.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Dima.Api.Handlers;

public class ReportHandler(AppDbContext context) : IReportHandler
{
    public async Task<Response<List<IncomesAndExpenses>?>> GetIncomesAndExpensesReportAsync(
        GetIncomesAndExpensesRequest request)
    {
        var (year, month) = GetReferenceMonth(request.Year, request.Month);

        // Last 12 months ending in the selected month (inclusive)
        var endDate = new DateTime(year, month, 1).AddMonths(1).AddTicks(-1);
        var startDate = new DateTime(year, month, 1).AddMonths(-11);

        try
        {
            var data = await context
                .Transactions
                .AsNoTracking()
                .Where(x =>
                    x.UserId == request.UserId
                    && x.PaidOrReceivedAt >= startDate
                    && x.PaidOrReceivedAt <= endDate)
                .GroupBy(x => new { x.PaidOrReceivedAt!.Value.Year, x.PaidOrReceivedAt!.Value.Month })
                .Select(g => new IncomesAndExpenses(
                    request.UserId,
                    g.Key.Month,
                    g.Key.Year,
                    g.Where(t => t.Type == ETransactionType.Deposit).Sum(t => t.Amount),
                    g.Where(t => t.Type == ETransactionType.Withdraw).Sum(t => t.Amount)))
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return new Response<List<IncomesAndExpenses>?>(data);
        }
        catch
        {
            return new Response<List<IncomesAndExpenses>?>(null, 500, "Não foi possível obter as entradas e saídas");
        }
    }

    public async Task<Response<List<IncomesByCategory>?>> GetIncomesByCategoryReportAsync(
        GetIncomesByCategoryRequest request)
    {
        var (year, month) = GetReferenceMonth(request.Year, request.Month);
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        try
        {
            var data = await context
                .Transactions
                .AsNoTracking()
                .Where(x =>
                    x.UserId == request.UserId
                    && x.Type == ETransactionType.Deposit
                    && x.PaidOrReceivedAt >= startDate
                    && x.PaidOrReceivedAt <= endDate)
                .GroupBy(x => x.Category.Title)
                .Select(g => new IncomesByCategory(
                    request.UserId,
                    g.Key,
                    year,
                    g.Sum(t => t.Amount)))
                .OrderBy(x => x.Category)
                .ToListAsync();

            return new Response<List<IncomesByCategory>?>(data);
        }
        catch
        {
            return new Response<List<IncomesByCategory>?>(null, 500,
                "Não foi possível obter as entradas por categoria");
        }
    }

    public async Task<Response<List<ExpensesByCategory>?>> GetExpensesByCategoryReportAsync(
        GetExpensesByCategoryRequest request)
    {
        var (year, month) = GetReferenceMonth(request.Year, request.Month);
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        try
        {
            var data = await context
                .Transactions
                .AsNoTracking()
                .Where(x =>
                    x.UserId == request.UserId
                    && x.Type == ETransactionType.Withdraw
                    && x.PaidOrReceivedAt >= startDate
                    && x.PaidOrReceivedAt <= endDate)
                .GroupBy(x => x.Category.Title)
                .Select(g => new ExpensesByCategory(
                    request.UserId,
                    g.Key,
                    year,
                    g.Sum(t => t.Amount)))
                .OrderBy(x => x.Category)
                .ToListAsync();

            return new Response<List<ExpensesByCategory>?>(data);
        }
        catch
        {
            return new Response<List<ExpensesByCategory>?>(null, 500,
                "Não foi possível obter as despesas por categoria");
        }
    }

    public async Task<Response<FinancialSummary?>> GetFinancialSummaryReportAsync(GetFinancialSummaryRequest request)
    {
        var (year, month) = GetReferenceMonth(request.Year, request.Month);
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        try
        {
            var data = await context
                .Transactions
                .AsNoTracking()
                .Where(
                    x => x.UserId == request.UserId
                         && x.PaidOrReceivedAt >= startDate
                         && x.PaidOrReceivedAt <= endDate
                )
                .GroupBy(x => 1)
                .Select(x => new FinancialSummary(
                    request.UserId,
                    x.Where(ty => ty.Type == ETransactionType.Deposit).Sum(t => t.Amount),
                    x.Where(ty => ty.Type == ETransactionType.Withdraw).Sum(t => t.Amount))
                )
                .FirstOrDefaultAsync();

            return new Response<FinancialSummary?>(data);
        }
        catch
        {
            return new Response<FinancialSummary?>(null, 500,
                "Não foi possível obter o resultado financeiro");
        }
    }

    private static (int Year, int Month) GetReferenceMonth(int? year, int? month)
    {
        var now = DateTime.Now;
        return (year ?? now.Year, month ?? now.Month);
    }
}
