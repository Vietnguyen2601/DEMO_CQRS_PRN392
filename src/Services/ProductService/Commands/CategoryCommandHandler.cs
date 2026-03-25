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

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
{
    private readonly ProductDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        ProductDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check if category with same name already exists
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingCategory != null)
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "CategoryCreated",
            aggregateId: category.Id,
            data: new { category.Id, category.Name, category.IsActive },
            cancellationToken);

        return MapToResponseDto(category);
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
                    AggregateType = "Category",
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

    private static CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
{
    private readonly ProductDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        ProductDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<CategoryResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Category with ID {request.Id} not found");

        if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name)
        {
            // Check if new name already exists
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.Id != request.Id, cancellationToken);

            if (existingCategory != null)
                throw new InvalidOperationException($"Category with name '{request.Name}' already exists");

            category.Name = request.Name;
        }

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "CategoryUpdated",
            aggregateId: category.Id,
            data: new { category.Id, category.Name, category.IsActive },
            cancellationToken);

        return MapToResponseDto(category);
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
                    AggregateType = "Category",
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

    private static CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ProductDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        ProductDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null)
            throw new InvalidOperationException($"Category with ID {request.Id} not found");

        // Check if category has products
        var productsCount = await _context.Products
            .CountAsync(p => p.CategoryId == request.Id, cancellationToken);

        if (productsCount > 0)
            throw new InvalidOperationException($"Cannot delete category with {productsCount} product(s)");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        await PublishKafkaEventSafe(
            eventType: "CategoryDeleted",
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
                    AggregateType = "Category",
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
