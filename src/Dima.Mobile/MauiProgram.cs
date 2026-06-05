using Dima.Core.Handlers;
using Dima.Mobile.Handlers;
using Dima.Mobile.Security;
using Microsoft.Extensions.Http.Resilience;
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
        builder.Services.AddTransient<IUserContext, UserContext>();

        builder.Services
            .AddHttpClient(Configuration.HttpClientName, opt =>
            {
                opt.BaseAddress = new Uri(Configuration.BackendUrl);
            })
            .AddHttpMessageHandler<BearerTokenHandler>()
            .AddStandardResilienceHandler(options =>
            {
                // Trata erros transitórios (DNS falhando no cold start do app,
                // SocketException, 5xx, 408, 429) tentando até 3 vezes com
                // backoff exponencial.
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.UseJitter = true;

                // Tolerância maior porque o Azure SQL Free hiberna (AutoPause)
                // e pode demorar 30s+ pra acordar na primeira request após 1h.
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);

                // O circuit breaker padrão usa janela de 30s; alinha com o
                // timeout por tentativa para evitar reclamação do validador.
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(2);
            });

        builder.Services.AddTransient<IAccountHandler, AccountHandler>();
        builder.Services.AddTransient<ICategoryHandler, CategoryHandler>();
        builder.Services.AddTransient<ITransactionHandler, TransactionHandler>();
        builder.Services.AddTransient<IReportHandler, ReportHandler>();

        return builder.Build();
    }
}
