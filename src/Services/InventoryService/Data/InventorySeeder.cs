using InventoryService.Models;

namespace InventoryService.Data;

public static class InventorySeeder
{
    public static void Seed(InventoryDbContext context)
    {
        if (context.InventoryItems.Any())
            return;

        var inventoryItems = new[]
        {
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop Pro 15",
                Quantity = 50,
                LastUpdated = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Wireless Mouse",
                Quantity = 200,
                LastUpdated = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "C# Programming Guide",
                Quantity = 75,
                LastUpdated = DateTime.UtcNow
            }
        };

        context.InventoryItems.AddRange(inventoryItems);
        context.SaveChanges();
    }
}
