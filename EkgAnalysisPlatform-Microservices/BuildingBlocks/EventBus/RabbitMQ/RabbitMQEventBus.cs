using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName = "ekg_event_bus";
        private readonly Dictionary<string, List<Type>> _handlers;
        
        public RabbitMQEventBus(string hostName)
        {
            _connectionFactory = new ConnectionFactory { HostName = hostName };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
            _handlers = new Dictionary<string, List<Type>>();
        }
        
        public void Publish<T>(T @event) where T : IntegrationEvent
        {
            var eventName = @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            
            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: eventName,
                basicProperties: null,
                body: body);
        }
        
        // Implement other methods...
        
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}