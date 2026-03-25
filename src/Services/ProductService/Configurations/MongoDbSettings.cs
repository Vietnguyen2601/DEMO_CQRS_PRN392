namespace ProductService.Configurations;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "CQRS_Product_Read";
    public string ProductsCollectionName { get; set; } = "products_read";
    public string CategoriesCollectionName { get; set; } = "categories_read";
}