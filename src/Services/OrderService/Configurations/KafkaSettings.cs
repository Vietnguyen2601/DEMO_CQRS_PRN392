namespace OrderService.Configurations;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string OrderEventsTopic { get; set; } = "order-events";
    public string ProductEventsTopic { get; set; } = "product-events";
    public string InventoryEventsTopic { get; set; } = "inventory-events";
    public string GroupId { get; set; } = "order-service-group";
}
