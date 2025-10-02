
using TechChallenge.Payments.Enum;
using TechChallenge.Payments.Services;

namespace TechChallenge.Payments.Tests.Services;

public class PaymentResultTests
{
    [Fact]
    public void DeveCriarResultadoAprovado_QuandoChamado()
    {
        // Arrange
        var authCode = "AUTH123";

        // Act
        var result = PaymentResult.Approved(authCode);

        // Assert
        Assert.Equal(PaymentStatus.Approved, result.Status);
        Assert.Equal(authCode, result.AuthorizationCode);
        Assert.Equal("Approved", result.Message);
    }

    [Fact]
    public void DeveCriarResultadoRecusado_QuandoChamado()
    {
        // Arrange
        var message = "Card declined";

        // Act
        var result = PaymentResult.Denied(message);

        // Assert
        Assert.Equal(PaymentStatus.Denied, result.Status);
        Assert.Null(result.AuthorizationCode);
        Assert.Equal(message, result.Message);
    }

    [Fact]
    public void DeveCriarResultadoPendente_QuandoChamado()
    {
        // Arrange
        var message = "Awaiting confirmation";

        // Act
        var result = PaymentResult.Pending(message);

        // Assert
        Assert.Equal(PaymentStatus.Pending, result.Status);
        Assert.Null(result.AuthorizationCode);
        Assert.Equal(message, result.Message);
    }

    [Fact]
    public void DeveCriarResultadoComErro_QuandoChamado()
    {
        // Arrange
        var message = "Gateway error";

        // Act
        var result = PaymentResult.Error(message);

        // Assert
        Assert.Equal(PaymentStatus.Error, result.Status);
        Assert.Null(result.AuthorizationCode);
        Assert.Equal(message, result.Message);
    }
}
