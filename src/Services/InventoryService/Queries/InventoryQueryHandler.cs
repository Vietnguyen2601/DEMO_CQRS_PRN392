using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Models;
using InventoryService.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Handlers;

public class GetAllInventoryItemsQueryHandler : IRequestHandler<GetAllInventoryItemsQuery, List<InventoryResponseDto>>
{
    private readonly InventoryDbContext _context;

    public GetAllInventoryItemsQueryHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryResponseDto>> Handle(GetAllInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.InventoryItems
            .OrderByDescending(x => x.LastUpdated)
            .ToListAsync(cancellationToken);

        return items.Select(MapToResponseDto).ToList();
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

public class GetInventoryItemByIdQueryHandler : IRequestHandler<GetInventoryItemByIdQuery, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;

    public GetInventoryItemByIdQueryHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryResponseDto> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

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

public class GetInventoryByProductIdQueryHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryResponseDto>
{
    private readonly InventoryDbContext _context;

    public GetInventoryByProductIdQueryHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryResponseDto> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory for product ID {request.ProductId} not found");

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
