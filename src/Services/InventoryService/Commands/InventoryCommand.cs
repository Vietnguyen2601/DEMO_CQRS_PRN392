using InventoryService.Dtos;
using MediatR;

namespace InventoryService.Commands;

public class CreateInventoryItemCommand : IRequest<InventoryResponseDto>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class UpdateInventoryItemCommand : IRequest<InventoryResponseDto>
{
    public Guid Id { get; set; }
    public string? ProductName { get; set; }
    public int? Quantity { get; set; }
}

public class UpdateInventoryQuantityCommand : IRequest<InventoryResponseDto>
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
}

public class DeleteInventoryItemCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
