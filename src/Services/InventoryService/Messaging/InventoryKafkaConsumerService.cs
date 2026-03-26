using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using InventoryService.Configurations;
using InventoryService.Data;
using InventoryService.Models;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Messaging;

public class InventoryKafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<InventoryKafkaConsumerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private const int RetryDelayMs = 2000;

    public InventoryKafkaConsumerService(
        IOptions<KafkaSettings> options,
        ILogger<InventoryKafkaConsumerService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _settings = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureTopicExistsAsync(_settings.ProductEventsTopic, stoppingToken);
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
        consumer.Subscribe(_settings.ProductEventsTopic);

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
                    _logger.LogWarning("Kafka consume retry for topic {Topic}: {Reason}", _settings.ProductEventsTopic, ex.Error.Reason);
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
                        HandleProductEvent(evt, stoppingToken).GetAwaiter().GetResult();

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Inventory Kafka consumer loop");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task HandleProductEvent(KafkaEventMessage evt, CancellationToken cancellationToken)
    {
        if (evt.Source != "ProductService")
            return;

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        if (evt.EventType == "ProductCreated")
        {
            var payload = JsonSerializer.Deserialize<ProductCreatedPayload>(evt.Data);
            if (payload == null || payload.Id == Guid.Empty)
                return;

            var exists = await dbContext.InventoryItems
                .AnyAsync(x => x.ProductId == payload.Id, cancellationToken);

            if (!exists)
            {
                dbContext.InventoryItems.Add(new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = payload.Id,
                    ProductName = payload.Name,
                    Quantity = 0,
                    LastUpdated = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Auto-created inventory for ProductId {ProductId}", payload.Id);
            }

            return;
        }

        if (evt.EventType == "ProductDeleted")
        {
            var payload = JsonSerializer.Deserialize<ProductDeletedPayload>(evt.Data);
            if (payload == null || payload.Id == Guid.Empty)
                return;

            var existing = await dbContext.InventoryItems
                .FirstOrDefaultAsync(x => x.ProductId == payload.Id, cancellationToken);

            if (existing != null)
            {
                dbContext.InventoryItems.Remove(existing);
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Auto-deleted inventory for ProductId {ProductId}", payload.Id);
            }
        }
    }

    private sealed class ProductCreatedPayload
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ProductDeletedPayload
    {
        public Guid Id { get; set; }
    }
}
