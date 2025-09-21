using TechChallenge.Payments.Enum;

namespace TechChallenge.Payments.Models;

public sealed class PaymentProcessedEvent
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public int JogoId { get; init; }
    public decimal Amount { get; init; }
    public PaymentStatus Status { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? Message { get; init; }
    public DateTime ProcessedAtUtc { get; init; }
}