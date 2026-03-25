using InventoryService.Dtos;
using MediatR;

namespace InventoryService.Queries;

public class GetAllInventoryItemsQuery : IRequest<List<InventoryResponseDto>>
{
}

public class GetInventoryItemByIdQuery : IRequest<InventoryResponseDto>
{
    public Guid Id { get; set; }

    public GetInventoryItemByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetInventoryByProductIdQuery : IRequest<InventoryResponseDto>
{
    public Guid ProductId { get; set; }

    public GetInventoryByProductIdQuery(Guid productId)
    {
        ProductId = productId;
    }
}
