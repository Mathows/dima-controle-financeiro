using Dima.Api;
using Dima.Api.Common.Api;
using Dima.Api.Data;
using Dima.Api.Endpoints;
using Dima.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfiguration();
builder.AddSecurity();
builder.AddDataContexts();
builder.AddCrossOrigin();
builder.AddDocumentation();
builder.AddServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    const int maxAttempts = 6;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            dbContext.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            var delay = TimeSpan.FromSeconds(attempt * 10);
            logger.LogWarning(ex,
                "Falha ao aplicar migrations (tentativa {Attempt}/{Max}). Aguardando {Delay}s antes de tentar novamente. Esse delay é esperado quando Azure SQL Free está saindo do AutoPause.",
                attempt, maxAttempts, delay.TotalSeconds);
            Thread.Sleep(delay);
        }
    }
}

if (app.Environment.IsDevelopment())
    app.ConfigureDevEnvironment();

app.UseCors(ApiConfiguration.CorsPolicyName);
app.UseSecurity();
app.MapEndpoints();

app.Run();