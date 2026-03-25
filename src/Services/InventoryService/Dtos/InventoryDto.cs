namespace InventoryService.Dtos;

public class InventoryResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class CreateInventoryItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class UpdateInventoryItemDto
{
    public string? ProductName { get; set; }
    public int? Quantity { get; set; }
}

public class UpdateInventoryQuantityDto
{
    public int Quantity { get; set; }
}
