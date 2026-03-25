using InventoryService.Commands;
using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Handlers;

public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;

    public CreateInventoryItemCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryResponseDto> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var existingItem = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);

        if (existingItem != null)
            throw new InvalidOperationException($"Inventory for product ID {request.ProductId} already exists");

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            LastUpdated = DateTime.UtcNow
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(item);
    }

    private static InventoryResponseDto MapToResponseDto(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            LastUpdated = item.LastUpdated
        };
    }
}

public class UpdateInventoryItemCommandHandler : IRequestHandler<UpdateInventoryItemCommand, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;

    public UpdateInventoryItemCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryResponseDto> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

        if (!string.IsNullOrWhiteSpace(request.ProductName))
            item.ProductName = request.ProductName;

        if (request.Quantity.HasValue)
            item.Quantity = request.Quantity.Value;

        item.LastUpdated = DateTime.UtcNow;

        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(item);
    }

    private static InventoryResponseDto MapToResponseDto(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            LastUpdated = item.LastUpdated
        };
    }
}

public class UpdateInventoryQuantityCommandHandler : IRequestHandler<UpdateInventoryQuantityCommand, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;

    public UpdateInventoryQuantityCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryResponseDto> Handle(UpdateInventoryQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

        item.Quantity = request.Quantity;
        item.LastUpdated = DateTime.UtcNow;

        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(item);
    }

    private static InventoryResponseDto MapToResponseDto(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            LastUpdated = item.LastUpdated
        };
    }
}

public class DeleteInventoryItemCommandHandler : IRequestHandler<DeleteInventoryItemCommand, bool>
{
    private readonly InventoryDbContext _context;

    public DeleteInventoryItemCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
