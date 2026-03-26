namespace OrderService.Models;

public class OrderItem
{
    public Guid OrderItemId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public double UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }

    // Navigation property
    public virtual Order Order { get; set; } = null!;
}
