namespace TechChallenge.Payments.Models;

public sealed class OrderPlacedEvent
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public Guid JogoId { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }
    public DateTime OccurredAtUtc { get; init; }
}