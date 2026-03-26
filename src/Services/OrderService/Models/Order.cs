namespace OrderService.Models;

public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public double TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
