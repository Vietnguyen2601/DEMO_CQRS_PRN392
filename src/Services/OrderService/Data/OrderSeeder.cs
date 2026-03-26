using OrderService.Models;

namespace OrderService.Data;

public static class OrderSeeder
{
    public static void Seed(OrderDbContext context)
    {
        // Only seed if database is empty
        if (context.Orders.Any())
            return;

        var orders = new List<Order>
        {
            new Order
            {
                OrderId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                TotalAmount = 250.00,
                DeliveryAddress = "123 Main Street, City, Country",
                CreatedAt = DateTime.UtcNow
            },
            new Order
            {
                OrderId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                TotalAmount = 150.50,
                DeliveryAddress = "456 Oak Avenue, Town, Country",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Orders.AddRange(orders);
        context.SaveChanges();

        // Add order items
        var orderItems = new List<OrderItem>
        {
            new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = orders[0].OrderId,
                ProductId = Guid.NewGuid(),
                UnitPrice = 50.00,
                Quantity = 3,
                TotalPrice = 150.00
            },
            new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = orders[0].OrderId,
                ProductId = Guid.NewGuid(),
                UnitPrice = 100.00,
                Quantity = 1,
                TotalPrice = 100.00
            },
            new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = orders[1].OrderId,
                ProductId = Guid.NewGuid(),
                UnitPrice = 150.50,
                Quantity = 1,
                TotalPrice = 150.50
            }
        };

        context.OrderItems.AddRange(orderItems);
        context.SaveChanges();
    }
}
