using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ProductService.Configurations;
using ProductService.Dtos;
using ProductService.Models;

namespace ProductService.Data;

public interface IProductReadRepository
{
    Task<List<ProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken);
    Task<ProductResponseDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ProductResponseDto>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<List<ProductResponseDto>> GetProductsByShopAsync(Guid shopId, CancellationToken cancellationToken);

    Task<List<CategoryResponseDto>> GetAllCategoriesAsync(CancellationToken cancellationToken);
    Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<CategoryResponseDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken);

    Task SyncFromWriteStoreAsync(IEnumerable<Product> products, IEnumerable<Category> categories, CancellationToken cancellationToken);
}

public class ProductReadRepository : IProductReadRepository
{
    private readonly IMongoCollection<ProductReadDocument> _products;
    private readonly IMongoCollection<CategoryReadDocument> _categories;

    public ProductReadRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var db = mongoClient.GetDatabase(settings.DatabaseName);
        _products = db.GetCollection<ProductReadDocument>(settings.ProductsCollectionName);
        _categories = db.GetCollection<CategoryReadDocument>(settings.CategoriesCollectionName);
    }

    public async Task<List<ProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        var docs = await _products.Find(Builders<ProductReadDocument>.Filter.Empty)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(MapProduct).ToList();
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var doc = await _products.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return doc == null ? null : MapProduct(doc);
    }

    public async Task<List<ProductResponseDto>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var docs = await _products.Find(x => x.CategoryId == categoryId).ToListAsync(cancellationToken);
        return docs.Select(MapProduct).ToList();
    }

    public async Task<List<ProductResponseDto>> GetProductsByShopAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var docs = await _products.Find(x => x.ShopId == shopId).ToListAsync(cancellationToken);
        return docs.Select(MapProduct).ToList();
    }

    public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        var docs = await _categories.Find(Builders<CategoryReadDocument>.Filter.Empty).ToListAsync(cancellationToken);
        return docs.Select(MapCategory).ToList();
    }

    public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var doc = await _categories.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return doc == null ? null : MapCategory(doc);
    }

    public async Task<List<CategoryResponseDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken)
    {
        var docs = await _categories.Find(x => x.IsActive).ToListAsync(cancellationToken);
        return docs.Select(MapCategory).ToList();
    }

    public async Task SyncFromWriteStoreAsync(IEnumerable<Product> products, IEnumerable<Category> categories, CancellationToken cancellationToken)
    {
        await _products.DeleteManyAsync(Builders<ProductReadDocument>.Filter.Empty, cancellationToken);
        await _categories.DeleteManyAsync(Builders<CategoryReadDocument>.Filter.Empty, cancellationToken);

        var categoryMap = categories.ToDictionary(
            c => c.Id,
            c => new CategoryReadDocument
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive
            });

        var productDocs = products.Select(p =>
        {
            categoryMap.TryGetValue(p.CategoryId, out var category);
            return new ProductReadDocument
            {
                Id = p.Id,
                ShopId = p.ShopId,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Category = category
            };
        }).ToList();

        if (categoryMap.Count > 0)
            await _categories.InsertManyAsync(categoryMap.Values, cancellationToken: cancellationToken);

        if (productDocs.Count > 0)
            await _products.InsertManyAsync(productDocs, cancellationToken: cancellationToken);
    }

    private static ProductResponseDto MapProduct(ProductReadDocument doc)
    {
        return new ProductResponseDto
        {
            Id = doc.Id,
            ShopId = doc.ShopId,
            CategoryId = doc.CategoryId,
            Name = doc.Name,
            Description = doc.Description,
            Price = doc.Price,
            StockQuantity = doc.StockQuantity,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt,
            Category = doc.Category == null ? null : new CategoryResponseDto
            {
                Id = doc.Category.Id,
                Name = doc.Category.Name,
                IsActive = doc.Category.IsActive
            }
        };
    }

    private static CategoryResponseDto MapCategory(CategoryReadDocument doc)
    {
        return new CategoryResponseDto
        {
            Id = doc.Id,
            Name = doc.Name,
            IsActive = doc.IsActive
        };
    }

    private sealed class ProductReadDocument
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonElement("shop_id")]
        public Guid ShopId { get; set; }

        [BsonElement("category_id")]
        public Guid CategoryId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("category")]
        public CategoryReadDocument? Category { get; set; }
    }

    private sealed class CategoryReadDocument
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("is_active")]
        public bool IsActive { get; set; }
    }
}