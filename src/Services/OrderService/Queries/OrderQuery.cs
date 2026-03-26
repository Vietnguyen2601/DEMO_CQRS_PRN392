using MediatR;
using OrderService.Dtos;

namespace OrderService.Queries;

/// <summary>
/// Query to get all orders
/// </summary>
public class GetAllOrdersQuery : IRequest<List<OrderResponseDto>>
{
}

/// <summary>
/// Query to get an order by ID
/// </summary>
public class GetOrderByIdQuery : IRequest<OrderResponseDto>
{
    public Guid OrderId { get; set; }

    public GetOrderByIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Query to get orders by account ID
/// </summary>
public class GetOrdersByAccountQuery : IRequest<List<OrderResponseDto>>
{
    public Guid AccountId { get; set; }

    public GetOrdersByAccountQuery(Guid accountId)
    {
        AccountId = accountId;
    }
}

/// <summary>
/// Query to get order item by ID
/// </summary>
public class GetOrderItemByIdQuery : IRequest<OrderItemResponseDto>
{
    public Guid OrderItemId { get; set; }

    public GetOrderItemByIdQuery(Guid orderItemId)
    {
        OrderItemId = orderItemId;
    }
}

/// <summary>
/// Query to get order items by order ID
/// </summary>
public class GetOrderItemsByOrderIdQuery : IRequest<List<OrderItemResponseDto>>
{
    public Guid OrderId { get; set; }

    public GetOrderItemsByOrderIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}
