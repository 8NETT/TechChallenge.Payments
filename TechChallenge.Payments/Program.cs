using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechChallenge.Payments.Contracts;
using TechChallenge.Payments.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddLogging();

builder.Services.AddSingleton<EventHubProducerClient>(sp =>
{
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("EventHubs");
    var cfg    = sp.GetRequiredService<IConfiguration>();

    var conn = cfg["EVENT_HUB_CONNECTION"];
    var hub  = cfg["PURCHASES_HUB_NAME"];

    if (string.IsNullOrWhiteSpace(conn))
    {
        logger.LogError("EVENT_HUB_CONNECTION ausente; não criaremos EventHubProducerClient.");
        return null!;
    }
    
    if (conn.Contains("EntityPath=", StringComparison.OrdinalIgnoreCase))
        return new EventHubProducerClient(conn);

    if (string.IsNullOrWhiteSpace(hub))
    {
        logger.LogError("PURCHASES_HUB_NAME ausente e a connection não tem EntityPath.");
        return null!;
    }

    return new EventHubProducerClient(conn, hub);
});

builder.Services.AddSingleton<IPaymentGateway, PaymentGateway>();

var host = builder.Build();
host.Run();