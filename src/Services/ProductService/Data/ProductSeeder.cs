using ProductService.Models;

namespace ProductService.Data;

public static class ProductSeeder
{
    public static void Seed(ProductDbContext context)
    {
        if (context.Categories.Any() || context.Products.Any())
            return;

        // Seed Categories
        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics", IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Clothing", IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Books", IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Home & Garden", IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Sports & Outdoors", IsActive = true }
        };

        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Seed Products
        var products = new[]
        {
            new Product
            {
                Id = Guid.NewGuid(),
                ShopId = Guid.NewGuid(),
                CategoryId = categories[0].Id,
                Name = "Laptop Pro 15",
                Description = "High-performance laptop with 16GB RAM and 512GB SSD",
                Price = 999.99m,
                StockQuantity = 50,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ShopId = Guid.NewGuid(),
                CategoryId = categories[0].Id,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with long battery life",
                Price = 29.99m,
                StockQuantity = 200,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ShopId = Guid.NewGuid(),
                CategoryId = categories[1].Id,
                Name = "Cotton T-Shirt",
                Description = "100% cotton comfortable t-shirt",
                Price = 19.99m,
                StockQuantity = 150,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ShopId = Guid.NewGuid(),
                CategoryId = categories[2].Id,
                Name = "C# Programming Guide",
                Description = "Complete guide to C# programming",
                Price = 49.99m,
                StockQuantity = 75,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ShopId = Guid.NewGuid(),
                CategoryId = categories[3].Id,
                Name = "Garden Tool Set",
                Description = "Professional 10-piece garden tool set",
                Price = 89.99m,
                StockQuantity = 30,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
