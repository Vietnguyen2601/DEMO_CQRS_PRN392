using MediatR;
using ProductService.Commands;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponseDto>
{
    private readonly ProductDbContext _context;

    public CreateProductCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponseDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            ShopId = request.ShopId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(product);
    }

    private static ProductResponseDto MapToResponseDto(Product product)
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
            UpdatedAt = product.UpdatedAt
        };
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponseDto>
{
    private readonly ProductDbContext _context;

    public UpdateProductCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponseDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found");

        // Validate category if being updated
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
                throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");
            product.CategoryId = request.CategoryId.Value;
        }

        if (!string.IsNullOrEmpty(request.Name))
            product.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Description))
            product.Description = request.Description;

        if (request.Price.HasValue)
            product.Price = request.Price.Value;

        if (request.StockQuantity.HasValue)
            product.StockQuantity = request.StockQuantity.Value;

        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(product);
    }

    private static ProductResponseDto MapToResponseDto(Product product)
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
            UpdatedAt = product.UpdatedAt
        };
    }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly ProductDbContext _context;

    public DeleteProductCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.Id} not found");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, ProductResponseDto>
{
    private readonly ProductDbContext _context;

    public UpdateProductStockCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponseDto> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found");

        product.StockQuantity = request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(product);
    }

    private static ProductResponseDto MapToResponseDto(Product product)
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
            UpdatedAt = product.UpdatedAt
        };
    }
}
