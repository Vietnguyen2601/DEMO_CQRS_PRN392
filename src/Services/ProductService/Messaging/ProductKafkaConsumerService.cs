using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using ProductService.Configurations;

namespace ProductService.Messaging;

public class ProductKafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<ProductKafkaConsumerService> _logger;

    public ProductKafkaConsumerService(IOptions<KafkaSettings> options, ILogger<ProductKafkaConsumerService> logger)
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
        consumer.Subscribe(_settings.InventoryEventsTopic);

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
                            "ProductService received Kafka event {EventType} from {Source} for {AggregateType}/{AggregateId}",
                            evt.EventType,
                            evt.Source,
                            evt.AggregateType,
                            evt.AggregateId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize inventory event from Kafka");
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
