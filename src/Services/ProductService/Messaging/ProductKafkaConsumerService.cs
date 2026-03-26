using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using ProductService.Configurations;

namespace ProductService.Messaging;

public class ProductKafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<ProductKafkaConsumerService> _logger;
    private const int RetryDelayMs = 2000;

    public ProductKafkaConsumerService(IOptions<KafkaSettings> options, ILogger<ProductKafkaConsumerService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureTopicExistsAsync(_settings.InventoryEventsTopic, stoppingToken);
        await Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private async Task EnsureTopicExistsAsync(string topicName, CancellationToken cancellationToken)
    {
        try
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = _settings.BootstrapServers
            }).Build();

            await admin.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            });

            _logger.LogInformation("Ensured Kafka topic exists: {Topic}", topicName);
        }
        catch (CreateTopicsException ex) when (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            _logger.LogInformation("Kafka topic already exists: {Topic}", topicName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create Kafka topic {Topic}. Consumer will retry.", topicName);
            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(RetryDelayMs, cancellationToken);
            }
        }
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
                ConsumeResult<string, string>? result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex) when (
                    ex.Error.Code == ErrorCode.UnknownTopicOrPart ||
                    ex.Error.Code == ErrorCode.Local_AllBrokersDown)
                {
                    _logger.LogWarning("Kafka consume retry for topic {Topic}: {Reason}", _settings.InventoryEventsTopic, ex.Error.Reason);
                    Task.Delay(RetryDelayMs, stoppingToken).Wait(stoppingToken);
                    continue;
                }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Product Kafka consumer loop");
        }
        finally
        {
            consumer.Close();
        }
    }
}
