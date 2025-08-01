using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PersistenciaService.Data;
using PersistenciaService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;

public class RabbitMQConsumerService : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly RabbitMQ.Client.IModel _channel;
    //private readonly IModel _channel;

    public RabbitMQConsumerService(IServiceScopeFactory scopeFactory) {
        _scopeFactory = scopeFactory;

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

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var contato = JsonSerializer.Deserialize<Contato>(json);

            if (contato != null) {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Contatos.Add(contato);
                await dbContext.SaveChangesAsync();
            }

            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        };


        _channel.BasicConsume(
            queue: "contatos-queue",
            autoAck: false,
            consumer: consumer
        );

        return Task.CompletedTask;
    }

    public override void Dispose() {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
