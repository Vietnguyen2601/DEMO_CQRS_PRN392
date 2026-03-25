using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Queries;
using MediatR;

namespace InventoryService.Handlers;

public class GetAllInventoryItemsQueryHandler : IRequestHandler<GetAllInventoryItemsQuery, List<InventoryResponseDto>>
{
    private readonly IInventoryReadRepository _readRepository;

    public GetAllInventoryItemsQueryHandler(IInventoryReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<List<InventoryResponseDto>> Handle(GetAllInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetAllAsync(cancellationToken);
    }
}

public class GetInventoryItemByIdQueryHandler : IRequestHandler<GetInventoryItemByIdQuery, InventoryResponseDto>
{
    private readonly IInventoryReadRepository _readRepository;

    public GetInventoryItemByIdQueryHandler(IInventoryReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<InventoryResponseDto> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _readRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory item with ID {request.Id} not found");

        return item;
    }
}

public class GetInventoryByProductIdQueryHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryResponseDto>
{
    private readonly IInventoryReadRepository _readRepository;

    public GetInventoryByProductIdQueryHandler(IInventoryReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<InventoryResponseDto> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _readRepository.GetByProductIdAsync(request.ProductId, cancellationToken)
            ?? throw new InvalidOperationException($"Inventory for product ID {request.ProductId} not found");

        return item;
    }
}
