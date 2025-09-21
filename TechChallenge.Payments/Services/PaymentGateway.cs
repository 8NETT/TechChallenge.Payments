using TechChallenge.Payments.Contracts;
using TechChallenge.Payments.Models;

namespace TechChallenge.Payments.Services;

public class PaymentGateway : IPaymentGateway
{
    private readonly Random _rng = new(Random.Shared.Next());

    public Task<PaymentResult> ChargeAsync(OrderPlacedEvent order, CancellationToken ct = default)
    {
        var roll = _rng.Next(0, 100);
        return Task.FromResult(roll switch
        {
            < 60 => PaymentResult.Approved(GenAuth()),
            < 85 => PaymentResult.Denied("Card declined by issuer"),
            < 95 => PaymentResult.Pending("Awaiting 3DS/async confirmation"),
            _    => PaymentResult.Error("Gateway unreachable (mock)")
        });

        static string GenAuth() => $"AUTH-{Guid.NewGuid():N}".ToUpper()[..16];
    }
}