namespace OrderService.Dtos;

public class OrderResponseDto
{
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public double TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemResponseDto> OrderItems { get; set; } = new();
}

public class CreateOrderDto
{
    public Guid AccountId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

public class UpdateOrderDto
{
    public string? DeliveryAddress { get; set; }
}
