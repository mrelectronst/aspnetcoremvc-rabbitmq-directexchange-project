using RabbitMQ.Client;

namespace AspNetCore.RabbitMQ.ExampleProject.Services.RabbitMQ
{
    public interface IRabbitMQClientService
    {
        IModel Connect();

    }

    public class RabbitMQClientService : IRabbitMQClientService, IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQClientService> _logger;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "imageDirectExchange";
        public static string RoutingWaterMark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();

            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);

            _channel.QueueDeclare(QueueName, true, false, false, null);

            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWaterMark);

            _logger.LogInformation("Connected to RabbitMQ");

            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("Disconnected to RabbitMQ");
        }
    }
}
