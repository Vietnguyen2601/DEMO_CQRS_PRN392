using MediatR;
using ProductService.Dtos;

namespace ProductService.Queries;

/// <summary>
/// Query to get all categories
/// </summary>
public class GetAllCategoriesQuery : IRequest<List<CategoryResponseDto>>
{
}

/// <summary>
/// Query to get a category by ID
/// </summary>
public class GetCategoryByIdQuery : IRequest<CategoryResponseDto>
{
    public Guid Id { get; set; }

    public GetCategoryByIdQuery(Guid id)
    {
        Id = id;
    }
}

/// <summary>
/// Query to get active categories only
/// </summary>
public class GetActiveCategoriesQuery : IRequest<List<CategoryResponseDto>>
{
}
