using MediatR;
using ProductService.Dtos;

namespace ProductService.Commands;

/// <summary>
/// Command to create a new product
/// </summary>
public class CreateProductCommand : IRequest<ProductResponseDto>
{
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

/// <summary>
/// Command to update an existing product
/// </summary>
public class UpdateProductCommand : IRequest<ProductResponseDto>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Command to delete a product
/// </summary>
public class DeleteProductCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Command to update product stock
/// </summary>
public class UpdateProductStockCommand : IRequest<ProductResponseDto>
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
}
