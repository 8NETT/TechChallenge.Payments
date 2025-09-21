using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TechChallenge.Payments.Models;
using TechChallenge.Payments.Contracts;

namespace TechChallenge.Payments.Functions;

public sealed class OrderPlacedHandler(
    IPaymentGateway gateway,
    EventHubProducerClient paymentsProducer,
    ILogger<OrderPlacedHandler> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<int> _processed = [];

    [Function("OrderPlacedHandler")]
    public async Task Run(
        [EventHubTrigger("%PURCHASES_HUB_NAME%", Connection = "EVENT_HUB_CONNECTION", ConsumerGroup = "%EVENT_HUB_CONSUMER_GROUP%")]
        EventData[] events,
        FunctionContext context,
        CancellationToken ct)
    {
        foreach (var ed in events)
        {
            try
            {
                var json = Encoding.UTF8.GetString(ed.EventBody);
                var type = ed.Properties.TryGetValue("type", out var t) ? t?.ToString() : null;

                if (!string.Equals(type, "OrderPlaced", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryDeserialize(json, out Envelope<OrderPlacedEvent>? env))
                    {
                        logger.LogWarning("Mensagem ignorada: tipo desconhecido. body={Body}", json);
                        continue;
                    }
                    await HandleAsync(env.Data, ct);
                }
                else
                {
                    if (!TryDeserialize(json, out Envelope<OrderPlacedEvent>? env))
                    {
                        logger.LogWarning("Envelope inválido para OrderPlaced. body={Body}", json);
                        continue;
                    }
                    await HandleAsync(env.Data, ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao processar evento OrderPlaced.");
                throw;
            }
        }
    }

    private async Task HandleAsync(OrderPlacedEvent evt, CancellationToken ct)
    {
        if (!_processed.Add(evt.OrderId)) return;

        var result = await gateway.ChargeAsync(evt, ct);

        var payment = new PaymentProcessedEvent
        {
            OrderId = evt.OrderId,
            UserId = evt.UserId,
            JogoId = evt.JogoId,
            Amount = evt.Total,
            Status = result.Status,
            AuthorizationCode = result.AuthorizationCode,
            Message = result.Message,
            ProcessedAtUtc = DateTime.UtcNow
        };

        var envelope = new { type = "PaymentProcessed", occurredAtUtc = DateTime.UtcNow, data = payment };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOpts);
        var outEvent = new EventData(bytes) { ContentType = "application/json" };
        outEvent.Properties["type"] = "PaymentProcessed";

        var options = new SendEventOptions { PartitionKey = evt.UserId.ToString() };
        await paymentsProducer.SendAsync(new[] { outEvent }, options, ct);
    }

    private static bool TryDeserialize<T>(string json, out T? obj)
    {
        try { obj = JsonSerializer.Deserialize<T>(json, JsonOpts); return obj is not null; }
        catch { obj = default; return false; }
    }
}