using Dima.Core.Handlers;
using Dima.Core.Requests.Reports;
using Microsoft.AspNetCore.Components;

namespace Dima.Mobile.Components.Reports;

public partial class ExpensesByCategoryChartComponent : ComponentBase
{
    [Parameter] public int? Year { get; set; }
    [Parameter] public int? Month { get; set; }

    public List<double> Data { get; set; } = [];
    public List<string> Labels { get; set; } = [];
    public bool IsLoading { get; set; } = true;

    [Inject] public IReportHandler Handler { get; set; } = null!;

    private int? _lastYear;
    private int? _lastMonth;

    protected override async Task OnParametersSetAsync()
    {
        if (Year == _lastYear && Month == _lastMonth && Data.Count > 0)
            return;

        _lastYear = Year;
        _lastMonth = Month;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        Data = [];
        Labels = [];

        var request = new GetExpensesByCategoryRequest { Year = Year, Month = Month };
        var result = await Handler.GetExpensesByCategoryReportAsync(request);

        if (result.IsSuccess && result.Data is not null)
        {
            foreach (var item in result.Data)
            {
                Labels.Add($"{item.Category} ({item.Expenses:C})");
                Data.Add(Math.Abs((double)item.Expenses));
            }
        }

        IsLoading = false;
        StateHasChanged();
    }
}
