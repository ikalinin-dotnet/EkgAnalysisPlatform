using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EkgAnalysisPlatform.BuildingBlocks.EventBus.RabbitMQ
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName = "ekg_event_bus";
        private readonly IServiceProvider? _serviceProvider;
        private readonly ILogger<RabbitMQEventBus>? _logger;
        private readonly ConcurrentDictionary<string, List<Type>> _handlers;
        private readonly List<string> _declaredQueues;

        public RabbitMQEventBus(string hostName, IServiceProvider? serviceProvider = null, ILogger<RabbitMQEventBus>? logger = null)
        {
            _connectionFactory = new ConnectionFactory { HostName = hostName };
            _serviceProvider = serviceProvider;
            _logger = logger;
            _handlers = new ConcurrentDictionary<string, List<Type>>();
            _declaredQueues = new List<string>();

            try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
                
                _logger?.LogInformation("RabbitMQ Event Bus initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize RabbitMQ Event Bus");
                throw;
            }
        }

        // Constructor for testing
        public RabbitMQEventBus(string hostName, IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _handlers = new ConcurrentDictionary<string, List<Type>>();
            _declaredQueues = new List<string>();

            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
        }

        public void Publish<T>(T @event) where T : IntegrationEvent
        {
            try
            {
                var eventName = @event.GetType().Name;
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: eventName,
                    basicProperties: null,
                    body: body);

                _logger?.LogDebug("Published event {EventName} with ID {EventId}", eventName, @event.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
                throw;
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            _logger?.LogInformation("Subscribing to event {EventName} with handler {HandlerType}", eventName, handlerType.Name);

            _handlers.AddOrUpdate(eventName,
                new List<Type> { handlerType },
                (key, existing) =>
                {
                    if (!existing.Contains(handlerType))
                    {
                        existing.Add(handlerType);
                    }
                    return existing;
                });

            var queueName = $"{eventName}_queue";
            if (!_declaredQueues.Contains(queueName))
            {
                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: queueName, exchange: _exchangeName, routingKey: eventName);
                _declaredQueues.Add(queueName);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        
                        await ProcessEvent(eventName, message);
                        
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error processing event {EventName}", eventName);
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            }
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            _logger?.LogInformation("Unsubscribing from event {EventName} with handler {HandlerType}", eventName, handlerType.Name);

            if (_handlers.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handlerType);
                if (handlers.Count == 0)
                {
                    _handlers.TryRemove(eventName, out _);
                }
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger?.LogDebug("Processing event {EventName}", eventName);

            if (_handlers.TryGetValue(eventName, out var handlerTypes) && _serviceProvider != null)
            {
                foreach (var handlerType in handlerTypes)
                {
                    var handler = _serviceProvider.GetService(handlerType);
                    if (handler == null) continue;

                    var eventType = Assembly.GetExecutingAssembly().GetTypes()
                        .FirstOrDefault(t => t.Name == eventName);

                    if (eventType != null)
                    {
                        var integrationEvent = JsonSerializer.Deserialize(message, eventType);
                        var method = handlerType.GetMethod("Handle");
                        if (method != null)
                        {
                            await (Task)method.Invoke(handler, new[] { integrationEvent })!;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                
                _logger?.LogInformation("RabbitMQ Event Bus disposed");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing RabbitMQ Event Bus");
            }
        }
    }
}