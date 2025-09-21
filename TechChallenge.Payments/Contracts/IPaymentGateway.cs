using TechChallenge.Payments.Models;
using TechChallenge.Payments.Services;

namespace TechChallenge.Payments.Contracts;

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(OrderPlacedEvent order, CancellationToken ct = default);
}