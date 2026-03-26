using MongoDB.Bson.Serialization.Attributes;
using OrderService.Dtos;

namespace OrderService.Data;

/// <summary>
/// MongoDB read model for Order - nested structure
/// </summary>
[BsonIgnoreExtraElements]
public class OrderReadDocument
{
    [BsonId]
    public Guid OrderId { get; set; }

    public Guid AccountId { get; set; }
    public double TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    
    [BsonElement("OrderItems")]
    public List<OrderItemReadDocument> OrderItems { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

[BsonIgnoreExtraElements]
public class OrderItemReadDocument
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public double UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
}
