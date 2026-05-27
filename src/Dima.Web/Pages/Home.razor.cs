using System.Globalization;
using Dima.Core.Common.Extensions;
using Dima.Core.Handlers;
using Dima.Core.Models;
using Dima.Core.Models.Reports;
using Dima.Core.Requests.Reports;
using Dima.Core.Requests.Transactions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dima.Web.Pages;

public partial class HomePage : ComponentBase
{
    #region Services

    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IReportHandler ReportHandler { get; set; } = null!;
    [Inject] public ITransactionHandler TransactionHandler { get; set; } = null!;

    #endregion

    #region Properties

    public bool ShowValues { get; set; } = true;

    public int SelectedYear { get; set; } = DateTime.Now.Year;
    public int SelectedMonth { get; set; } = DateTime.Now.Month;

    public int[] Years { get; set; } =
    [
        DateTime.Now.AddYears(1).Year,
        DateTime.Now.Year,
        DateTime.Now.AddYears(-1).Year,
        DateTime.Now.AddYears(-2).Year,
        DateTime.Now.AddYears(-3).Year
    ];

    public IReadOnlyList<MonthOption> Months { get; } =
    [
        new(1, "Janeiro"), new(2, "Fevereiro"), new(3, "Março"),
        new(4, "Abril"), new(5, "Maio"), new(6, "Junho"),
        new(7, "Julho"), new(8, "Agosto"), new(9, "Setembro"),
        new(10, "Outubro"), new(11, "Novembro"), new(12, "Dezembro")
    ];

    public bool IsLoadingSummary { get; set; }
    public bool IsLoadingTransactions { get; set; }

    public FinancialSummary? Summary { get; set; }
    public List<Transaction> Transactions { get; set; } = [];

    public record MonthOption(int Number, string Name);

    #endregion

    #region Overrides

    protected override async Task OnInitializedAsync()
        => await LoadPeriodAsync();

    #endregion

    #region Methods

    public void ToggleShowValues()
        => ShowValues = !ShowValues;

    public async Task OnPeriodChangedAsync()
    {
        await LoadPeriodAsync();
        StateHasChanged();
    }

    private async Task LoadPeriodAsync()
    {
        await Task.WhenAll(LoadSummaryAsync(), LoadTransactionsAsync());
    }

    private async Task LoadSummaryAsync()
    {
        IsLoadingSummary = true;
        Summary = null;
        try
        {
            var request = new GetFinancialSummaryRequest
            {
                Year = SelectedYear,
                Month = SelectedMonth
            };
            var result = await ReportHandler.GetFinancialSummaryReportAsync(request);
            Summary = result.IsSuccess
                ? result.Data ?? new FinancialSummary(string.Empty, 0, 0)
                : new FinancialSummary(string.Empty, 0, 0);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
            Summary = new FinancialSummary(string.Empty, 0, 0);
        }
        finally
        {
            IsLoadingSummary = false;
        }
    }

    private async Task LoadTransactionsAsync()
    {
        IsLoadingTransactions = true;
        try
        {
            var referenceDate = new DateTime(SelectedYear, SelectedMonth, 1);
            var request = new GetTransactionsByPeriodRequest
            {
                StartDate = referenceDate.GetFirstDay(),
                EndDate = referenceDate.GetLastDay(),
                PageNumber = 1,
                PageSize = 1000
            };
            var result = await TransactionHandler.GetByPeriodAsync(request);
            Transactions = result.IsSuccess ? result.Data ?? [] : [];
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
            Transactions = [];
        }
        finally
        {
            IsLoadingTransactions = false;
        }
    }

    protected static string FormatTransactionDate(DateTime? date)
        => date?.ToString("dd 'de' MMM", CultureInfo.CurrentCulture) ?? "-";

    #endregion
}
