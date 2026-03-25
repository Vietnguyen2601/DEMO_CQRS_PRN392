using InventoryService.Configurations;
using InventoryService.Dtos;
using InventoryService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace InventoryService.Data;

public interface IInventoryReadRepository
{
    Task<List<InventoryResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<InventoryResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<InventoryResponseDto?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task UpsertAsync(InventoryItem item, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task SyncFromWriteStoreAsync(IEnumerable<InventoryItem> items, CancellationToken cancellationToken);
}

public class InventoryReadRepository : IInventoryReadRepository
{
    private readonly IMongoCollection<InventoryReadDocument> _collection;

    public InventoryReadRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> mongoOptions)
    {
        var settings = mongoOptions.Value;
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<InventoryReadDocument>(settings.InventoryCollectionName);
    }

    public async Task<List<InventoryResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var docs = await _collection.Find(Builders<InventoryReadDocument>.Filter.Empty)
            .SortByDescending(x => x.LastUpdated)
            .ToListAsync(cancellationToken);

        return docs.Select(MapToResponseDto).ToList();
    }

    public async Task<InventoryResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return doc == null ? null : MapToResponseDto(doc);
    }

    public async Task<InventoryResponseDto?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        var doc = await _collection.Find(x => x.ProductId == productId).FirstOrDefaultAsync(cancellationToken);
        return doc == null ? null : MapToResponseDto(doc);
    }

    public async Task UpsertAsync(InventoryItem item, CancellationToken cancellationToken)
    {
        var doc = new InventoryReadDocument
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            LastUpdated = item.LastUpdated
        };

        await _collection.ReplaceOneAsync(
            x => x.Id == item.Id,
            doc,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
    }

    public async Task SyncFromWriteStoreAsync(IEnumerable<InventoryItem> items, CancellationToken cancellationToken)
    {
        await _collection.DeleteManyAsync(Builders<InventoryReadDocument>.Filter.Empty, cancellationToken);

        var docs = items.Select(x => new InventoryReadDocument
        {
            Id = x.Id,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Quantity = x.Quantity,
            LastUpdated = x.LastUpdated
        }).ToList();

        if (docs.Count > 0)
            await _collection.InsertManyAsync(docs, cancellationToken: cancellationToken);
    }

    private static InventoryResponseDto MapToResponseDto(InventoryReadDocument doc)
    {
        return new InventoryResponseDto
        {
            Id = doc.Id,
            ProductId = doc.ProductId,
            ProductName = doc.ProductName,
            Quantity = doc.Quantity,
            LastUpdated = doc.LastUpdated
        };
    }

    private sealed class InventoryReadDocument
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonElement("product_id")]
        public Guid ProductId { get; set; }

        [BsonElement("product_name")]
        public string ProductName { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}