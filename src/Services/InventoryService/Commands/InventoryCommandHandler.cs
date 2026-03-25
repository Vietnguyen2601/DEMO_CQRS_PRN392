using InventoryService.Commands;
using InventoryService.Configurations;
using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Messaging;
using InventoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InventoryService.Handlers;

public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<CreateInventoryItemCommandHandler> _logger;

    public CreateInventoryItemCommandHandler(
        InventoryDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<CreateInventoryItemCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<InventoryResponseDto> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var existingItem = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);

        if (existingItem != null)
            throw new InvalidOperationException($"Inventory for product ID {request.ProductId} already exists");

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            LastUpdated = DateTime.UtcNow
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "InventoryCreated",
            aggregateId: item.Id,
            data: new { item.Id, item.ProductId, item.ProductName, item.Quantity },
            cancellationToken);

        return MapToResponseDto(item);
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.InventoryEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "InventoryItem",
                    AggregateId = aggregateId.ToString(),
                    Source = "InventoryService",
                    Data = JsonSerializer.Serialize(data),
                    OccurredAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kafka publish failed for event {EventType}", eventType);
        }
    }

    private static InventoryResponseDto MapToResponseDto(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            LastUpdated = item.LastUpdated
        };
    }
}

public class UpdateInventoryItemCommandHandler : IRequestHandler<UpdateInventoryItemCommand, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<UpdateInventoryItemCommandHandler> _logger;

    public UpdateInventoryItemCommandHandler(
        InventoryDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<UpdateInventoryItemCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<InventoryResponseDto> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

        if (!string.IsNullOrWhiteSpace(request.ProductName))
            item.ProductName = request.ProductName;

        if (request.Quantity.HasValue)
            item.Quantity = request.Quantity.Value;

        item.LastUpdated = DateTime.UtcNow;

        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "InventoryUpdated",
            aggregateId: item.Id,
            data: new { item.Id, item.ProductId, item.ProductName, item.Quantity },
            cancellationToken);

        return MapToResponseDto(item);
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.InventoryEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "InventoryItem",
                    AggregateId = aggregateId.ToString(),
                    Source = "InventoryService",
                    Data = JsonSerializer.Serialize(data),
                    OccurredAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kafka publish failed for event {EventType}", eventType);
        }
    }

    private static InventoryResponseDto MapToResponseDto(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            LastUpdated = item.LastUpdated
        };
    }
}

public class UpdateInventoryQuantityCommandHandler : IRequestHandler<UpdateInventoryQuantityCommand, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<UpdateInventoryQuantityCommandHandler> _logger;

    public UpdateInventoryQuantityCommandHandler(
        InventoryDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<UpdateInventoryQuantityCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<InventoryResponseDto> Handle(UpdateInventoryQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

        item.Quantity = request.Quantity;
        item.LastUpdated = DateTime.UtcNow;

        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "InventoryQuantityUpdated",
            aggregateId: item.Id,
            data: new { item.Id, item.ProductId, item.Quantity },
            cancellationToken);

        return MapToResponseDto(item);
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.InventoryEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "InventoryItem",
                    AggregateId = aggregateId.ToString(),
                    Source = "InventoryService",
                    Data = JsonSerializer.Serialize(data),
                    OccurredAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kafka publish failed for event {EventType}", eventType);
        }
    }

    private static InventoryResponseDto MapToResponseDto(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            LastUpdated = item.LastUpdated
        };
    }
}

public class DeleteInventoryItemCommandHandler : IRequestHandler<DeleteInventoryItemCommand, bool>
{
    private readonly InventoryDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<DeleteInventoryItemCommandHandler> _logger;

    public DeleteInventoryItemCommandHandler(
        InventoryDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<DeleteInventoryItemCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "InventoryDeleted",
            aggregateId: item.Id,
            data: new { item.Id, item.ProductId },
            cancellationToken);

        return true;
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.InventoryEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "InventoryItem",
                    AggregateId = aggregateId.ToString(),
                    Source = "InventoryService",
                    Data = JsonSerializer.Serialize(data),
                    OccurredAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kafka publish failed for event {EventType}", eventType);
        }
    }
}
