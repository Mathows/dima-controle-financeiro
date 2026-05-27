using System.Web;
using Dima.Core.Handlers;
using Dima.Core.Requests.Account;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dima.Web.Pages.Identity;

public partial class ResetPasswordPage : ComponentBase
{
    #region Services

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    public IAccountHandler Handler { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Properties

    public bool IsBusy { get; set; }
    public bool Success { get; set; }
    public bool TokenMissing { get; set; }
    public ResetPasswordRequest InputModel { get; set; } = new();

    #endregion

    #region Overrides

    protected override void OnInitialized()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var email = query["email"] ?? string.Empty;
        var code = query["code"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
        {
            TokenMissing = true;
            return;
        }

        InputModel.Email = email;
        InputModel.ResetCode = code;
    }

    #endregion

    #region Methods

    public async Task OnValidSubmitAsync()
    {
        IsBusy = true;
        try
        {
            var result = await Handler.ResetPasswordAsync(InputModel);
            if (result.IsSuccess)
            {
                Success = true;
            }
            else
            {
                Snackbar.Add(result.Message, Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion
}
