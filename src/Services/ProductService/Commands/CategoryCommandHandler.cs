using MediatR;
using ProductService.Commands;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Handlers;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
{
    private readonly ProductDbContext _context;

    public CreateCategoryCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check if category with same name already exists
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingCategory != null)
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(category);
    }

    private static CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
{
    private readonly ProductDbContext _context;

    public UpdateCategoryCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Category with ID {request.Id} not found");

        if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name)
        {
            // Check if new name already exists
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.Id != request.Id, cancellationToken);

            if (existingCategory != null)
                throw new InvalidOperationException($"Category with name '{request.Name}' already exists");

            category.Name = request.Name;
        }

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(category);
    }

    private static CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ProductDbContext _context;

    public DeleteCategoryCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null)
            throw new InvalidOperationException($"Category with ID {request.Id} not found");

        // Check if category has products
        var productsCount = await _context.Products
            .CountAsync(p => p.CategoryId == request.Id, cancellationToken);

        if (productsCount > 0)
            throw new InvalidOperationException($"Cannot delete category with {productsCount} product(s)");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
