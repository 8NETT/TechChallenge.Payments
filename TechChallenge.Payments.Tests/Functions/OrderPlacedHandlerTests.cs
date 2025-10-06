
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using TechChallenge.Payments.Contracts;
using TechChallenge.Payments.Enum;
using TechChallenge.Payments.Functions;
using TechChallenge.Payments.Models;
using TechChallenge.Payments.Services;

namespace TechChallenge.Payments.Tests.Functions;

public class OrderPlacedHandlerTests
{
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<EventHubProducerClient> _producerMock;
    private readonly Mock<ILogger<OrderPlacedHandler>> _loggerMock;
    private readonly OrderPlacedHandler _handler;

    public OrderPlacedHandlerTests()
    {
        _gatewayMock = new Mock<IPaymentGateway>();
        _producerMock = new Mock<EventHubProducerClient>();
        _loggerMock = new Mock<ILogger<OrderPlacedHandler>>();
        _handler = new OrderPlacedHandler(_gatewayMock.Object, _producerMock.Object, _loggerMock.Object);

        // Clear the static _processed set before each test
        var processedField = typeof(OrderPlacedHandler).GetField("_processed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var processedSet = (System.Collections.Generic.HashSet<int>)processedField.GetValue(null);
        processedSet.Clear();
    }

    [Fact]
    public async Task DeveProcessarEventoComSucesso_QuandoGatewayAprovaPagamento()
    {
        // Arrange
        var orderEvent = new OrderPlacedEvent { OrderId = 1, UserId = 123, JogoId = 456, Total = 100 };
        var paymentResult = PaymentResult.Approved("AUTH456");
        var eventData = CreateEventData(orderEvent, "OrderPlaced");

        _gatewayMock.Setup(g => g.ChargeAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        // Act
        await _handler.Run([eventData], Mock.Of<FunctionContext>(), CancellationToken.None);

        // Assert
        _gatewayMock.Verify(g => g.ChargeAsync(It.Is<OrderPlacedEvent>(e => e.OrderId == 1), It.IsAny<CancellationToken>()), Times.Once);
        _producerMock.Verify(p => p.SendAsync(It.IsAny<IEnumerable<EventData>>(), It.IsAny<SendEventOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveIgnorarEvento_QuandoNaoForOrderPlaced()
    {
        // Arrange
        var orderEvent = new { Message = "Some other event" };
        var eventData = CreateEventData(orderEvent, "SomeOtherType");

        // Act
        await _handler.Run([eventData], Mock.Of<FunctionContext>(), CancellationToken.None);

        // Assert
        _gatewayMock.Verify(g => g.ChargeAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeveLancarExcecao_QuandoGatewayRetornaErro()
    {
        // Arrange
        var orderEvent = new OrderPlacedEvent { OrderId = 1, UserId = 123, JogoId = 456, Total = 100 };
        var paymentResult = PaymentResult.Error("Gateway error");
        var eventData = CreateEventData(orderEvent, "OrderPlaced");

        _gatewayMock.Setup(g => g.ChargeAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Run([eventData], Mock.Of<FunctionContext>(), CancellationToken.None));
    }

    [Fact]
    public async Task DeveIgnorarEvento_QuandoJaProcessado()
    {
        // Arrange
        var orderEvent = new OrderPlacedEvent { OrderId = 1, UserId = 123, JogoId = 456, Total = 100 };
        var paymentResult = PaymentResult.Approved("AUTH456");
        var eventData = CreateEventData(orderEvent, "OrderPlaced");

        _gatewayMock.Setup(g => g.ChargeAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        // Act
        await _handler.Run([eventData], Mock.Of<FunctionContext>(), CancellationToken.None); // First call
        await _handler.Run([eventData], Mock.Of<FunctionContext>(), CancellationToken.None); // Second call

        // Assert
        _gatewayMock.Verify(g => g.ChargeAsync(It.Is<OrderPlacedEvent>(e => e.OrderId == 1), It.IsAny<CancellationToken>()), Times.Once); // Should only be called once
        _producerMock.Verify(p => p.SendAsync(It.IsAny<IEnumerable<EventData>>(), It.IsAny<SendEventOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveProcessarEventoComSucesso_QuandoPagamentoPendente()
    {
        // Arrange
        var orderEvent = new OrderPlacedEvent { OrderId = 1, UserId = 123, JogoId = 456, Total = 100 };
        var paymentResult = PaymentResult.Pending("Awaiting confirmation");
        var eventData = CreateEventData(orderEvent, "OrderPlaced");

        _gatewayMock.Setup(g => g.ChargeAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        // Act
        await _handler.Run([eventData], Mock.Of<FunctionContext>(), CancellationToken.None);

        // Assert
        _gatewayMock.Verify(g => g.ChargeAsync(It.Is<OrderPlacedEvent>(e => e.OrderId == 1), It.IsAny<CancellationToken>()), Times.Once);
        _producerMock.Verify(p => p.SendAsync(It.IsAny<IEnumerable<EventData>>(), It.IsAny<SendEventOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveProcessarEventoComSucesso_QuandoPagamentoRecusado()
    {
        // Arrange
        var orderEvent = new OrderPlacedEvent { OrderId = 1, UserId = 123, JogoId = 456, Total = 100 };
        var paymentResult = PaymentResult.Denied("Card declined");
        var eventData = CreateEventData(orderEvent, "OrderPlaced");

        _gatewayMock.Setup(g => g.ChargeAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        // Act
        await _handler.Run([eventData], Mock.Of<FunctionContext>(), CancellationToken.None);

        // Assert
        _gatewayMock.Verify(g => g.ChargeAsync(It.Is<OrderPlacedEvent>(e => e.OrderId == 1), It.IsAny<CancellationToken>()), Times.Once);
        _producerMock.Verify(p => p.SendAsync(It.IsAny<IEnumerable<EventData>>(), It.IsAny<SendEventOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static EventData CreateEventData(object data, string type)
    {
        var envelope = new { type, occurredAtUtc = DateTime.UtcNow, data };
        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var bytes = Encoding.UTF8.GetBytes(json);
        var eventData = new EventData(bytes);
        eventData.Properties["type"] = type;
        return eventData;
    }
}
