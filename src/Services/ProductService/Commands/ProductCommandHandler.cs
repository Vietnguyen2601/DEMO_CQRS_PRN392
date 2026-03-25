using MediatR;
using ProductService.Commands;
using ProductService.Configurations;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Messaging;
using ProductService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ProductService.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponseDto>
{
    private readonly ProductDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        ProductDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<CreateProductCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<ProductResponseDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            ShopId = request.ShopId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "ProductCreated",
            aggregateId: product.Id,
            data: new { product.Id, product.ShopId, product.CategoryId, product.Name, product.Price, product.StockQuantity },
            cancellationToken);

        return MapToResponseDto(product);
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.ProductEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "Product",
                    AggregateId = aggregateId.ToString(),
                    Source = "ProductService",
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

    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponseDto>
{
    private readonly ProductDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        ProductDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<ProductResponseDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found");

        // Validate category if being updated
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
                throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");
            product.CategoryId = request.CategoryId.Value;
        }

        if (!string.IsNullOrEmpty(request.Name))
            product.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Description))
            product.Description = request.Description;

        if (request.Price.HasValue)
            product.Price = request.Price.Value;

        if (request.StockQuantity.HasValue)
            product.StockQuantity = request.StockQuantity.Value;

        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "ProductUpdated",
            aggregateId: product.Id,
            data: new { product.Id, product.ShopId, product.CategoryId, product.Name, product.Price, product.StockQuantity },
            cancellationToken);

        return MapToResponseDto(product);
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.ProductEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "Product",
                    AggregateId = aggregateId.ToString(),
                    Source = "ProductService",
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

    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly ProductDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        ProductDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.Id} not found");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "ProductDeleted",
            aggregateId: request.Id,
            data: new { Id = request.Id },
            cancellationToken);

        return true;
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.ProductEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "Product",
                    AggregateId = aggregateId.ToString(),
                    Source = "ProductService",
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

public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, ProductResponseDto>
{
    private readonly ProductDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<UpdateProductStockCommandHandler> _logger;

    public UpdateProductStockCommandHandler(
        ProductDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<UpdateProductStockCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<ProductResponseDto> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found");

        product.StockQuantity = request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "ProductStockUpdated",
            aggregateId: product.Id,
            data: new { product.Id, product.StockQuantity },
            cancellationToken);

        return MapToResponseDto(product);
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.ProductEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "Product",
                    AggregateId = aggregateId.ToString(),
                    Source = "ProductService",
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

    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
