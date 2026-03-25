using MediatR;
using ProductService.Queries;
using ProductService.Data;
using ProductService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Handlers;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductResponseDto>>
{
    private readonly ProductDbContext _context;

    public GetAllProductsQueryHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponseDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponseDto).ToList();
    }

    private static ProductResponseDto MapToResponseDto(Models.Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Category = product.Category != null ? new CategoryResponseDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                IsActive = product.Category.IsActive
            } : null
        };
    }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto>
{
    private readonly ProductDbContext _context;

    public GetProductByIdQueryHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponseDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found");

        return MapToResponseDto(product);
    }

    private static ProductResponseDto MapToResponseDto(Models.Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Category = product.Category != null ? new CategoryResponseDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                IsActive = product.Category.IsActive
            } : null
        };
    }
}

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductResponseDto>>
{
    private readonly ProductDbContext _context;

    public GetProductsByCategoryQueryHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Where(p => p.CategoryId == request.CategoryId)
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponseDto).ToList();
    }

    private static ProductResponseDto MapToResponseDto(Models.Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Category = product.Category != null ? new CategoryResponseDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                IsActive = product.Category.IsActive
            } : null
        };
    }
}

public class GetProductsByShopQueryHandler : IRequestHandler<GetProductsByShopQuery, List<ProductResponseDto>>
{
    private readonly ProductDbContext _context;

    public GetProductsByShopQueryHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsByShopQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Where(p => p.ShopId == request.ShopId)
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponseDto).ToList();
    }

    private static ProductResponseDto MapToResponseDto(Models.Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Category = product.Category != null ? new CategoryResponseDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                IsActive = product.Category.IsActive
            } : null
        };
    }
}
