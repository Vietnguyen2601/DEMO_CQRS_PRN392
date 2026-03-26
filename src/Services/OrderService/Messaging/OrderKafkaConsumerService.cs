using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrderService.Configurations;

namespace OrderService.Messaging;

public class OrderKafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<OrderKafkaConsumerService> _logger;

    public OrderKafkaConsumerService(IOptions<KafkaSettings> options, ILogger<OrderKafkaConsumerService> logger)
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
        // Subscribe to product and inventory events
        consumer.Subscribe(new[] { _settings.ProductEventsTopic, _settings.InventoryEventsTopic });

        _logger.LogInformation("OrderKafkaConsumerService started, listening to topics: {Topics}",
            string.Join(", ", _settings.ProductEventsTopic, _settings.InventoryEventsTopic));

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
                            "OrderService received Kafka event {EventType} from {Source} for {AggregateType}/{AggregateId}",
                            evt.EventType,
                            evt.Source,
                            evt.AggregateType,
                            evt.AggregateId);

                        // Handle events based on type
                        HandleEvent(evt);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize event from Kafka");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("OrderKafkaConsumerService stopped");
        }
        finally
        {
            consumer.Close();
        }
    }

    private void HandleEvent(KafkaEventMessage evt)
    {
        switch (evt.EventType)
        {
            case "ProductCreated":
                _logger.LogInformation("Product created: {Data}", evt.Data);
                break;

            case "ProductUpdated":
                _logger.LogInformation("Product updated: {Data}", evt.Data);
                break;

            case "ProductDeleted":
                _logger.LogInformation("Product deleted: {AggregateId}", evt.AggregateId);
                break;

            case "InventoryCreated":
                _logger.LogInformation("Inventory created: {Data}", evt.Data);
                break;

            case "InventoryUpdated":
                _logger.LogInformation("Inventory updated: {Data}", evt.Data);
                break;

            default:
                _logger.LogWarning("Unknown event type: {EventType}", evt.EventType);
                break;
        }
    }
}
