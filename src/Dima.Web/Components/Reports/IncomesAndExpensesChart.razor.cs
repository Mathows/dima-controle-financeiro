using System.Globalization;
using Dima.Core.Handlers;
using Dima.Core.Requests.Reports;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dima.Web.Components.Reports;

public partial class IncomesAndExpensesChartComponent : ComponentBase
{
    #region Properties

    public ChartOptions Options { get; set; } = new();
    public List<ChartSeries>? Series { get; set; }
    public List<string> Labels { get; set; } = [];

    #endregion

    #region Services

    [Inject]
    public IReportHandler Handler { get; set; } = null!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    #endregion

    #region Override

    protected override async Task OnInitializedAsync()
    {
        var request = new GetIncomesAndExpensesRequest();
        var result = await Handler.GetIncomesAndExpensesReportAsync(request);
        if (!result.IsSuccess || result.Data is null)
        {
            Series = [];
            return;
        }

        var incomes = new List<double>();
        var expenses = new List<double>();

        foreach (var item in result.Data.OrderBy(x => x.Year).ThenBy(x => x.Month))
        {
            incomes.Add((double)item.Incomes);
            expenses.Add(Math.Abs((double)item.Expenses));
            Labels.Add(GetMonthName(item.Month));
        }

        Options.YAxisTicks = 1000;
        Options.YAxisFormat = "0,K";
        Options.ChartPalette = ["#76FF01", Colors.Red.Default];
        Options.InterpolationOption = InterpolationOption.NaturalSpline;

        Series =
        [
            new ChartSeries { Name = "Receitas", Data = incomes.ToArray() },
            new ChartSeries { Name = "Saídas", Data = expenses.ToArray() }
        ];

        StateHasChanged();
    }

    #endregion

    private static string GetMonthName(int month)
        => new DateTime(DateTime.Now.Year, month, 1)
            .ToString("MMMM", CultureInfo.CurrentCulture);
}