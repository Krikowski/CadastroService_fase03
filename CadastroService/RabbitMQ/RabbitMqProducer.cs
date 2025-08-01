using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CadastroService.RabbitMQ {
    public class RabbitMQPublisherService : IDisposable {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQPublisherService() {
            var factory = new ConnectionFactory() {
                HostName = "rabbitmq" 
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "contatos-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        public void PublishContato(object contato) {
            var json = JsonSerializer.Serialize(contato);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "contatos-queue",
                basicProperties: null,
                body: body
            );
        }

        public void Dispose() {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
