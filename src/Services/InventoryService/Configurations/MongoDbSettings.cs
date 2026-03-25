namespace InventoryService.Configurations;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "CQRS_Inventory_Read";
    public string InventoryCollectionName { get; set; } = "inventory_items_read";
}