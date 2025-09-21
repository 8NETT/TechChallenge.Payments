using TechChallenge.Payments.Enum;

namespace TechChallenge.Payments.Services;

public sealed class PaymentResult
{
    public PaymentStatus Status { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? Message { get; init; }

    public static PaymentResult Approved(string auth) => new() { Status = PaymentStatus.Approved, AuthorizationCode = auth, Message = "Approved" };
    public static PaymentResult Denied(string msg)    => new() { Status = PaymentStatus.Denied,  Message = msg };
    public static PaymentResult Pending(string msg)   => new() { Status = PaymentStatus.Pending, Message = msg };
    public static PaymentResult Error(string msg)     => new() { Status = PaymentStatus.Error,   Message = msg };
}