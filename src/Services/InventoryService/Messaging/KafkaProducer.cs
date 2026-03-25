using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Configurations;
using Microsoft.Extensions.Options;

namespace InventoryService.Messaging;

public interface IKafkaProducer
{
    Task PublishAsync(string topic, string key, KafkaEventMessage message, CancellationToken cancellationToken);
}

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IOptions<KafkaSettings> kafkaOptions)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.Value.BootstrapServers,
            Acks = Acks.Leader
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(string topic, string key, KafkaEventMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = payload
        }, cancellationToken);
    }
}
