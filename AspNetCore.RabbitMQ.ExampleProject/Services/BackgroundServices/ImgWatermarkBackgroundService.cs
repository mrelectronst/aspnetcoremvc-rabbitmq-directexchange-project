using AspNetCore.RabbitMQ.ExampleProject.Services.RabbitMQ;
using AspNetCore.RabbitMQ.ExampleProject.Services.RabbitMQ.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace AspNetCore.RabbitMQ.ExampleProject.Services.BackgroundServices
{
    public class ImgWatermarkBackgroundService : BackgroundService
    {
        private readonly IRabbitMQClientService _rabbitMQClientService;
        private IModel channel;

        public ImgWatermarkBackgroundService(IRabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            channel = _rabbitMQClientService.Connect();

            channel.BasicQos(0, 1, false);



            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subcriber = new AsyncEventingBasicConsumer(channel);

            channel.BasicConsume(RabbitMQClientService.QueueName, false, subcriber);

            subcriber.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var imageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", imageCreatedEvent.ImageName);

                var siteName = "www.mysite.com";

                using var img = Image.FromFile(path);

                using var graphic = Graphics.FromImage(img);

                var font = new Font(FontFamily.GenericMonospace, 15, FontStyle.Bold, GraphicsUnit.Pixel);

                var textSize = graphic.MeasureString(siteName, font);

                var color = Color.FromArgb(128, 255, 255, 255);

                var brush = new SolidBrush(color);

                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 40));

                graphic.DrawString(siteName, font, brush, position);

                img.Save("wwwroot/Images/watermarks/" + imageCreatedEvent.ImageName);

                img.Dispose();

                graphic.Dispose();

                channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                throw;
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
