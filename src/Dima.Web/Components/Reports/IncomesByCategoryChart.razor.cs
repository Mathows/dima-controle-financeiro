using Dima.Core.Handlers;
using Dima.Core.Requests.Reports;
using Microsoft.AspNetCore.Components;

namespace Dima.Web.Components.Reports;

public partial class IncomesByCategoryChartComponent : ComponentBase
{
    #region Parameters

    [Parameter] public int? Year { get; set; }
    [Parameter] public int? Month { get; set; }

    #endregion

    #region Properties

    public List<double> Data { get; set; } = [];
    public List<string> Labels { get; set; } = [];
    public bool IsLoading { get; set; } = true;

    #endregion

    #region Services

    [Inject] public IReportHandler Handler { get; set; } = null!;

    #endregion

    #region State

    private int? _lastYear;
    private int? _lastMonth;

    #endregion

    #region Lifecycle

    protected override async Task OnParametersSetAsync()
    {
        if (Year == _lastYear && Month == _lastMonth && Data.Count > 0)
            return;

        _lastYear = Year;
        _lastMonth = Month;
        await LoadAsync();
    }

    #endregion

    #region Methods

    private async Task LoadAsync()
    {
        IsLoading = true;
        Data = [];
        Labels = [];

        var request = new GetIncomesByCategoryRequest
        {
            Year = Year,
            Month = Month
        };
        var result = await Handler.GetIncomesByCategoryReportAsync(request);

        if (result.IsSuccess && result.Data is not null)
        {
            foreach (var item in result.Data)
            {
                Labels.Add($"{item.Category} ({item.Incomes:C})");
                Data.Add((double)item.Incomes);
            }
        }

        IsLoading = false;
        StateHasChanged();
    }

    #endregion
}
