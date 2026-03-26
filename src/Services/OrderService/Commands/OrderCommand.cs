using MediatR;
using OrderService.Dtos;

namespace OrderService.Commands;

/// <summary>
/// Command to create a new order
/// </summary>
public class CreateOrderCommand : IRequest<OrderResponseDto>
{
    public Guid AccountId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

/// <summary>
/// Command to update an existing order
/// </summary>
public class UpdateOrderCommand : IRequest<OrderResponseDto>
{
    public Guid OrderId { get; set; }
    public string? DeliveryAddress { get; set; }
}

/// <summary>
/// Command to delete an order
/// </summary>
public class DeleteOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
}

/// <summary>
/// Command to add order items
/// </summary>
public class AddOrderItemCommand : IRequest<OrderResponseDto>
{
    public Guid OrderId { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

/// <summary>
/// Command to remove order item
/// </summary>
public class RemoveOrderItemCommand : IRequest<bool>
{
    public Guid OrderItemId { get; set; }
}
