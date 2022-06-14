using AspNetCore.RabbitMQ.ExampleProject.Services.RabbitMQ.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AspNetCore.RabbitMQ.ExampleProject.Services.RabbitMQ
{
    public interface IRabbitMQPublisher
    {
        void Publish(ProductImageCreatedEvent productImageCreatedEvent);
    }

    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IRabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(IRabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(ProductImageCreatedEvent productImageCreatedEvent)
        {
            var channel = _rabbitMQClientService.Connect();

            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();

            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingWaterMark,
                                    basicProperties: properties, body: bodyByte);
        }
    }
}
