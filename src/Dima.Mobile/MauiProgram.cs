using Dima.Core.Handlers;
using Dima.Mobile.Handlers;
using Dima.Mobile.Security;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace Dima.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddMudServices();

        builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        builder.Services.AddTransient<BearerTokenHandler>();

        builder.Services
            .AddHttpClient(Configuration.HttpClientName, opt =>
            {
                opt.BaseAddress = new Uri(Configuration.BackendUrl);
            })
            .AddHttpMessageHandler<BearerTokenHandler>();

        builder.Services.AddTransient<IAccountHandler, AccountHandler>();
        builder.Services.AddTransient<ICategoryHandler, CategoryHandler>();
        builder.Services.AddTransient<ITransactionHandler, TransactionHandler>();
        builder.Services.AddTransient<IReportHandler, ReportHandler>();

        return builder.Build();
    }
}
