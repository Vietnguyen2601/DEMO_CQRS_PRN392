namespace OrderService.Configurations;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "CQRS_Order_Read";
    public string OrdersCollectionName { get; set; } = "orders_read";
}
