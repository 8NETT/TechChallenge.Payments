using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TechChallenge.Payments.Contracts;
using TechChallenge.Payments.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() 
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        // Mock gateway
        services.AddSingleton<IPaymentGateway, PaymentGateway>();

        // Event Hub Producer para publicar PaymentProcessed
        var paymentsConnStr = cfg["EVENT_HUB_CONNECTION"]
                              ?? throw new InvalidOperationException("EVENT_HUB_CONNECTION missing");
        var paymentsHubName = cfg["PAYMENTS_HUB_NAME"]
                              ?? throw new InvalidOperationException("PAYMENTS_HUB_NAME missing");

        services.AddSingleton(new EventHubProducerClient(paymentsConnStr, paymentsHubName));
    })
    .Build();

await host.RunAsync();