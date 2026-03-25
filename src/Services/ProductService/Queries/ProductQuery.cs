using MediatR;
using ProductService.Dtos;

namespace ProductService.Queries;

/// <summary>
/// Query to get all products
/// </summary>
public class GetAllProductsQuery : IRequest<List<ProductResponseDto>>
{
}

/// <summary>
/// Query to get a product by ID
/// </summary>
public class GetProductByIdQuery : IRequest<ProductResponseDto>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}

/// <summary>
/// Query to get products by category ID
/// </summary>
public class GetProductsByCategoryQuery : IRequest<List<ProductResponseDto>>
{
    public Guid CategoryId { get; set; }

    public GetProductsByCategoryQuery(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}

/// <summary>
/// Query to get products by shop ID
/// </summary>
public class GetProductsByShopQuery : IRequest<List<ProductResponseDto>>
{
    public Guid ShopId { get; set; }

    public GetProductsByShopQuery(Guid shopId)
    {
        ShopId = shopId;
    }
}
