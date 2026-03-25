using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Configurations;
using Microsoft.Extensions.Options;

namespace InventoryService.Messaging;

public class InventoryKafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<InventoryKafkaConsumerService> _logger;

    public InventoryKafkaConsumerService(IOptions<KafkaSettings> options, ILogger<InventoryKafkaConsumerService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.ProductEventsTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value == null)
                    continue;

                try
                {
                    var evt = JsonSerializer.Deserialize<KafkaEventMessage>(result.Message.Value);
                    if (evt != null)
                    {
                        _logger.LogInformation(
                            "InventoryService received Kafka event {EventType} from {Source} for {AggregateType}/{AggregateId}",
                            evt.EventType,
                            evt.Source,
                            evt.AggregateType,
                            evt.AggregateId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize product event from Kafka");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        finally
        {
            consumer.Close();
        }
    }
}
