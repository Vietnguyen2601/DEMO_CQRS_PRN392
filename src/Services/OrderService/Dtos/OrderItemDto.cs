namespace OrderService.Dtos;

public class OrderItemResponseDto
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public double UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public double UnitPrice { get; set; }
    public int Quantity { get; set; }
}
