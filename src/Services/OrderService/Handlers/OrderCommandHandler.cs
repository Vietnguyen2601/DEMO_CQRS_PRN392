using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.Commands;
using OrderService.Configurations;
using OrderService.Data;
using OrderService.Dtos;
using OrderService.Messaging;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        OrderDbContext context,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for account {AccountId}", request.AccountId);

        // Calculate total amount
        var totalAmount = request.OrderItems.Sum(item => item.UnitPrice * item.Quantity);

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            AccountId = request.AccountId,
            TotalAmount = totalAmount,
            DeliveryAddress = request.DeliveryAddress,
            CreatedAt = DateTime.UtcNow
        };

        // Add order items
        foreach (var itemDto in request.OrderItems)
        {
            var orderItem = new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = order.OrderId,
                ProductId = itemDto.ProductId,
                UnitPrice = itemDto.UnitPrice,
                Quantity = itemDto.Quantity,
                TotalPrice = itemDto.UnitPrice * itemDto.Quantity
            };
            order.OrderItems.Add(orderItem);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish Kafka event
        await PublishKafkaEventSafe(
            eventType: "OrderCreated",
            aggregateId: order.OrderId,
            data: new { order.OrderId, order.AccountId, order.TotalAmount, ItemCount = order.OrderItems.Count },
            cancellationToken);

        _logger.LogInformation("Order {OrderId} created successfully", order.OrderId);

        return MapToResponseDto(order);
    }

    private async Task PublishKafkaEventSafe(string eventType, Guid aggregateId, object data, CancellationToken cancellationToken)
    {
        try
        {
            await _kafkaProducer.PublishAsync(
                _kafkaSettings.OrderEventsTopic,
                aggregateId.ToString(),
                new KafkaEventMessage
                {
                    EventType = eventType,
                    AggregateType = "Order",
                    AggregateId = aggregateId.ToString(),
                    Source = "OrderService",
                    Data = JsonSerializer.Serialize(data),
                    OccurredAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish Kafka event {EventType}", eventType);
        }
    }

    private OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<UpdateOrderCommandHandler> _logger;

    public UpdateOrderCommandHandler(OrderDbContext context, ILogger<UpdateOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating order {OrderId}", request.OrderId);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        if (!string.IsNullOrEmpty(request.DeliveryAddress))
            order.DeliveryAddress = request.DeliveryAddress;

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} updated successfully", order.OrderId);

        return MapToResponseDto(order);
    }

    private OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(OrderDbContext context, ILogger<DeleteOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting order {OrderId}", request.OrderId);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} deleted successfully", order.OrderId);

        return true;
    }
}

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, OrderResponseDto>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<AddOrderItemCommandHandler> _logger;

    public AddOrderItemCommandHandler(OrderDbContext context, ILogger<AddOrderItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding items to order {OrderId}", request.OrderId);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        foreach (var itemDto in request.OrderItems)
        {
            var orderItem = new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = order.OrderId,
                ProductId = itemDto.ProductId,
                UnitPrice = itemDto.UnitPrice,
                Quantity = itemDto.Quantity,
                TotalPrice = itemDto.UnitPrice * itemDto.Quantity
            };
            order.OrderItems.Add(orderItem);
        }

        // Recalculate total
        order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Items added to order {OrderId} successfully", order.OrderId);

        return MapToResponseDto(order);
    }

    private OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}

public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand, bool>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<RemoveOrderItemCommandHandler> _logger;

    public RemoveOrderItemCommandHandler(OrderDbContext context, ILogger<RemoveOrderItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing order item {OrderItemId}", request.OrderItemId);

        var orderItem = await _context.OrderItems.FirstOrDefaultAsync(oi => oi.OrderItemId == request.OrderItemId, cancellationToken);

        if (orderItem == null)
            throw new InvalidOperationException($"Order item with ID {request.OrderItemId} not found");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId, cancellationToken);

        _context.OrderItems.Remove(orderItem);

        if (order != null)
        {
            order.TotalAmount = order.OrderItems.Where(oi => oi.OrderItemId != request.OrderItemId).Sum(oi => oi.TotalPrice);
            order.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order item {OrderItemId} removed successfully", request.OrderItemId);

        return true;
    }
}
