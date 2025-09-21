using System.Text.Json.Serialization;

namespace TechChallenge.Payments.Models;

public sealed class Envelope<T>
{
    [JsonPropertyName("type")] public required string Type { get; init; }
    [JsonPropertyName("occurredAtUtc")] public DateTime OccurredAtUtc { get; init; }
    [JsonPropertyName("data")] public required T Data { get; init; }
}