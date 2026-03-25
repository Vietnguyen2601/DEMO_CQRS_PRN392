namespace ProductService.Configurations;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ProductEventsTopic { get; set; } = "product-events";
    public string InventoryEventsTopic { get; set; } = "inventory-events";
    public string GroupId { get; set; } = "product-service-group";
}
