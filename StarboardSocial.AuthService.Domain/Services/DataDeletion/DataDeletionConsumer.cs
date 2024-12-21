

using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace StarboardSocial.UserService.Domain.Services;

public class DataDeletionConsumer(IChannel channel, IDataDeletionService dataDeletionService)
{
    private readonly IChannel _channel = channel;
    private readonly IDataDeletionService _dataDeletionService = dataDeletionService;
    
    public async Task StartListening(Func<string, Task> onMessageReceived)
    {
        await _channel.ExchangeDeclareAsync(
            "data-deletion",
            ExchangeType.Topic,
            true,
            false);
        
        QueueDeclareOk queueDeclareResult = await _channel.QueueDeclareAsync();
        string queueName = queueDeclareResult.QueueName;

        await _channel.QueueBindAsync(queue: queueName, exchange: "data-deletion", routingKey: "#" );
        
        AsyncEventingBasicConsumer consumer = new(_channel);
        
        consumer.ReceivedAsync += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            string routingKey = ea.RoutingKey;
            Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
            await onMessageReceived(message);
            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        await _channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);
    }
}