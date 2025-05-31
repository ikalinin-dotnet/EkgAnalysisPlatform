using System;
using System.Text;
using System.Text.Json;
using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace EkgAnalysisPlatform.BuildingBlocks.Tests.EventBus
{
    public class IntegrationEventTests
    {
        [Fact]
        public void Constructor_SetsIdAndCreationDate()
        {
            // Act
            var testEvent = new TestIntegrationEvent();
            
            // Assert
            testEvent.Id.Should().NotBe(Guid.Empty);
            testEvent.CreationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        // Test integration event class
        private class TestIntegrationEvent : IntegrationEvent
        {
            public string TestProperty { get; set; } = "Test";
        }
    }
    
    public class RabbitMQEventBusTests
    {
        private readonly Mock<IConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IConnection> _connectionMock;
        private readonly Mock<IModel> _channelMock;
        
        public RabbitMQEventBusTests()
        {
            _connectionFactoryMock = new Mock<IConnectionFactory>();
            _connectionMock = new Mock<IConnection>();
            _channelMock = new Mock<IModel>();
            
            _connectionFactoryMock
                .Setup(cf => cf.CreateConnection())
                .Returns(_connectionMock.Object);
                
            _connectionMock
                .Setup(c => c.CreateModel())
                .Returns(_channelMock.Object);
        }
        
        [Fact]
        public void Constructor_InitializesEventBus()
        {
            // Arrange & Act
            using var eventBus = new RabbitMQEventBus("localhost", _connectionFactoryMock.Object);
            
            // Assert
            _connectionFactoryMock.Verify(cf => cf.CreateConnection(), Times.Once);
            _connectionMock.Verify(c => c.CreateModel(), Times.Once);
            _channelMock.Verify(ch => ch.ExchangeDeclare(
                It.Is<string>(s => s == "ekg_event_bus"), 
                It.Is<string>(s => s == ExchangeType.Direct),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>()),
                Times.Once);
        }
        
        [Fact]
        public void Publish_PublishesEventToExchange()
        {
            // Arrange
            using var eventBus = new RabbitMQEventBus("localhost", _connectionFactoryMock.Object);
            var testEvent = new TestIntegrationEvent { TestProperty = "TestValue" };
            
            // Act
            eventBus.Publish(testEvent);
            
            // Assert
            var eventJson = JsonSerializer.Serialize(testEvent);
            var eventBytes = Encoding.UTF8.GetBytes(eventJson);
            
            _channelMock.Verify(ch => ch.BasicPublish(
                It.Is<string>(s => s == "ekg_event_bus"),
                It.Is<string>(s => s == "TestIntegrationEvent"),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.Is<ReadOnlyMemory<byte>>(b => AssertMessageBody(b, eventBytes))),
                Times.Once);
        }
        
        [Fact]
        public void Dispose_DisposesConnectionAndChannel()
        {
            // Arrange
            var eventBus = new RabbitMQEventBus("localhost", _connectionFactoryMock.Object);
            
            // Act
            eventBus.Dispose();
            
            // Assert
            _channelMock.Verify(ch => ch.Dispose(), Times.Once);
            _connectionMock.Verify(c => c.Dispose(), Times.Once);
        }
        
        // Helper method to compare message body bytes
        private bool AssertMessageBody(ReadOnlyMemory<byte> actual, byte[] expected)
        {
            if (actual.Length != expected.Length)
                return false;
                
            var actualSpan = actual.Span;
            for (int i = 0; i < expected.Length; i++)
            {
                if (actualSpan[i] != expected[i])
                    return false;
            }
            
            return true;
        }
        
        // Test integration event class
        private class TestIntegrationEvent : IntegrationEvent
        {
            public string TestProperty { get; set; } = "Test";
        }
        
        // Test integration event handler
        private class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
        {
            public bool Handled { get; private set; }
            
            public Task Handle(TestIntegrationEvent @event)
            {
                Handled = true;
                return Task.CompletedTask;
            }
        }
    }
    
    public class EventBusSubscriptionManagerTests
    {
        // This would test a subscription manager class if one was implemented
        // For now, we'll assume the RabbitMQ implementation handles subscriptions directly
    }
}