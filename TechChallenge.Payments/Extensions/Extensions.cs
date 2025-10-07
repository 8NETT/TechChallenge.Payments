using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting
{
    public static class Extensions
    {
        public static IHostApplicationBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.ConfigureOpenTelemetry();

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter("Microsoft.Azure.Functions.Worker");
                })
                .WithTracing(tracing =>
                {
                    tracing
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                        })
                        .AddSource("Microsoft.Azure.Functions.Worker");
                });

            return builder;
        }

        public static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.Services.AddOpenTelemetry()
                    .UseAzureMonitor();
            }

            return builder;
        }
    }
}
