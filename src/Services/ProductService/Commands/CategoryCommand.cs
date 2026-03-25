using MediatR;
using ProductService.Dtos;

namespace ProductService.Commands;

/// <summary>
/// Command to create a new category
/// </summary>
public class CreateCategoryCommand : IRequest<CategoryResponseDto>
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Command to update an existing category
/// </summary>
public class UpdateCategoryCommand : IRequest<CategoryResponseDto>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Command to delete a category
/// </summary>
public class DeleteCategoryCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
