using MediatR;
using ProductService.Queries;
using ProductService.Data;
using ProductService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Handlers;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryResponseDto>>
{
    private readonly ProductDbContext _context;

    public GetAllCategoriesQueryHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponseDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories.ToListAsync(cancellationToken);
        return categories.Select(MapToResponseDto).ToList();
    }

    private static CategoryResponseDto MapToResponseDto(Models.Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryResponseDto>
{
    private readonly ProductDbContext _context;

    public GetCategoryByIdQueryHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryResponseDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Category with ID {request.Id} not found");

        return MapToResponseDto(category);
    }

    private static CategoryResponseDto MapToResponseDto(Models.Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }
}

public class GetActiveCategoriesQueryHandler : IRequestHandler<GetActiveCategoriesQuery, List<CategoryResponseDto>>
{
    private readonly ProductDbContext _context;

    public GetActiveCategoriesQueryHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponseDto>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        return categories.Select(MapToResponseDto).ToList();
    }

    private static CategoryResponseDto MapToResponseDto(Models.Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }
}
