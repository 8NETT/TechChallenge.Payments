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

builder.Services.AddOpenTelemetry();

builder.Services.AddSingleton<EventHubProducerClient>(sp =>
{
    var cfg    = sp.GetRequiredService<IConfiguration>();

    var conn = cfg["EVENT_HUB_CONNECTION"];
    var hub  = cfg["PURCHASES_HUB_NAME"];

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("EVENT_HUB_CONNECTION is not configured.");

    if (conn.Contains("EntityPath=", StringComparison.OrdinalIgnoreCase))
        return new EventHubProducerClient(conn);

    if (string.IsNullOrWhiteSpace(hub))
        throw new InvalidOperationException("PURCHASES_HUB_NAME is missing and the connection has no EntityPath.");

    return new EventHubProducerClient(conn, hub);
});

builder.Services.AddSingleton<IPaymentGateway, PaymentGateway>();

var host = builder.Build();
host.Run();