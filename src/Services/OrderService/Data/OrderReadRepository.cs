using OrderService.Configurations;
using OrderService.Dtos;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace OrderService.Data;

public interface IOrderReadRepository
{
    Task<List<OrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderResponseDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<OrderResponseDto>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<OrderItemResponseDto?> GetOrderItemByIdAsync(Guid orderItemId, CancellationToken cancellationToken = default);
    Task<List<OrderItemResponseDto>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task UpsertAsync(Models.Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task SyncFromWriteStoreAsync(IEnumerable<Models.Order> orders, CancellationToken cancellationToken = default);
}

public class OrderReadRepository : IOrderReadRepository
{
    private readonly IMongoCollection<OrderReadDocument> _collection;

    public OrderReadRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> mongoOptions)
    {
        var settings = mongoOptions.Value;
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<OrderReadDocument>(settings.OrdersCollectionName);
    }

    public async Task<List<OrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var docs = await _collection.Find(Builders<OrderReadDocument>.Filter.Empty)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(MapToResponseDto).ToList();
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var doc = await _collection.Find(x => x.OrderId == orderId)
            .FirstOrDefaultAsync(cancellationToken);

        return doc == null ? null : MapToResponseDto(doc);
    }

    public async Task<List<OrderResponseDto>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var docs = await _collection.Find(x => x.AccountId == accountId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(MapToResponseDto).ToList();
    }

    public async Task<OrderItemResponseDto?> GetOrderItemByIdAsync(Guid orderItemId, CancellationToken cancellationToken = default)
    {
        // Find order containing this item
        var doc = await _collection.Find(
            Builders<OrderReadDocument>.Filter.ElemMatch(x => x.OrderItems,
                Builders<OrderItemReadDocument>.Filter.Eq(oi => oi.OrderItemId, orderItemId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (doc == null) return null;

        var item = doc.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);
        return item == null ? null : MapOrderItemToResponseDto(item);
    }

    public async Task<List<OrderItemResponseDto>> GetOrderItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var doc = await _collection.Find(x => x.OrderId == orderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (doc == null) return new List<OrderItemResponseDto>();

        return doc.OrderItems.Select(MapOrderItemToResponseDto).ToList();
    }

    public async Task UpsertAsync(Models.Order order, CancellationToken cancellationToken = default)
    {
        var doc = MapToReadDocument(order);

        var filter = Builders<OrderReadDocument>.Filter.Eq(x => x.OrderId, order.OrderId);

        await _collection.ReplaceOneAsync(
            filter,
            doc,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public async Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OrderReadDocument>.Filter.Eq(x => x.OrderId, orderId);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task SyncFromWriteStoreAsync(IEnumerable<Models.Order> orders, CancellationToken cancellationToken = default)
    {
        var docs = orders.Select(MapToReadDocument).ToList();

        if (docs.Count == 0) return;

        // Clear and re-insert for complete sync
        await _collection.DeleteManyAsync(Builders<OrderReadDocument>.Filter.Empty, cancellationToken);

        if (docs.Count > 0)
        {
            await _collection.InsertManyAsync(docs, cancellationToken: cancellationToken);
        }
    }

    private OrderReadDocument MapToReadDocument(Models.Order order)
    {
        return new OrderReadDocument
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            OrderItems = order.OrderItems.Select(oi => new OrderItemReadDocument
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    private OrderResponseDto MapToResponseDto(OrderReadDocument doc)
    {
        return new OrderResponseDto
        {
            OrderId = doc.OrderId,
            AccountId = doc.AccountId,
            TotalAmount = doc.TotalAmount,
            DeliveryAddress = doc.DeliveryAddress,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt,
            OrderItems = doc.OrderItems.Select(MapOrderItemToResponseDto).ToList()
        };
    }

    private OrderItemResponseDto MapOrderItemToResponseDto(OrderItemReadDocument item)
    {
        return new OrderItemResponseDto
        {
            OrderItemId = item.OrderItemId,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice
        };
    }
}
