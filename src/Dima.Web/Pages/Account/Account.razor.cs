using Dima.Core.Handlers;
using Dima.Core.Requests.Account;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dima.Web.Pages.Account;

public partial class AccountPage : ComponentBase
{
    #region Services

    [Inject] public IAccountHandler Handler { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    #endregion

    #region Properties

    public bool IsLoading { get; set; } = true;
    public bool IsSavingProfile { get; set; }
    public bool IsChangingPassword { get; set; }

    public string CurrentEmail { get; set; } = string.Empty;
    public UpdateProfileRequest ProfileModel { get; set; } = new();
    public ChangePasswordRequest PasswordModel { get; set; } = new();

    #endregion

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await Handler.GetProfileAsync();
            if (result.IsSuccess && result.Data is not null)
            {
                ProfileModel.FirstName = result.Data.FirstName;
                ProfileModel.LastName = result.Data.LastName;
                CurrentEmail = result.Data.Email;
            }
            else
            {
                Snackbar.Add(result.Message ?? "Não foi possível carregar o perfil", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Methods

    public async Task OnUpdateProfileAsync()
    {
        IsSavingProfile = true;
        try
        {
            var result = await Handler.UpdateProfileAsync(ProfileModel);
            if (result.IsSuccess && result.Data is not null)
            {
                ProfileModel.FirstName = result.Data.FirstName;
                ProfileModel.LastName = result.Data.LastName;
                Snackbar.Add("Perfil atualizado! Faça login novamente para ver o nome atualizado no menu.", Severity.Success);
            }
            else
            {
                Snackbar.Add(result.Message ?? "Não foi possível atualizar o perfil", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            IsSavingProfile = false;
        }
    }

    public async Task OnChangePasswordAsync()
    {
        IsChangingPassword = true;
        try
        {
            var result = await Handler.ChangePasswordAsync(PasswordModel);
            if (result.IsSuccess)
            {
                PasswordModel = new ChangePasswordRequest();
                Snackbar.Add("Senha alterada com sucesso!", Severity.Success);
            }
            else
            {
                Snackbar.Add(result.Message ?? "Não foi possível alterar a senha", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    #endregion
}
