using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Commands;
using ProductService.Dtos;
using ProductService.Queries;

namespace ProductService.Controllers;

/// <summary>
/// Category Management API Controller
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns>List of all categories</returns>
    /// <response code="200">Categories retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetAllCategories()
    {
        try
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery());
            return Ok(ApiResponse<List<CategoryResponseDto>>.SuccessResponse(categories, "Categories retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve categories", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get active categories only
    /// </summary>
    /// <returns>List of active categories</returns>
    /// <response code="200">Active categories retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetActiveCategories()
    {
        try
        {
            var categories = await _mediator.Send(new GetActiveCategoriesQuery());
            return Ok(ApiResponse<List<CategoryResponseDto>>.SuccessResponse(categories, "Active categories retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve categories", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery(id));
            return Ok(ApiResponse<CategoryResponseDto>.SuccessResponse(category, "Category retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve category", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="dto">Category creation details</param>
    /// <returns>Created category details</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/categories
    ///     {
    ///       "name": "Electronics"
    ///     }
    /// </remarks>
    /// <response code="201">Category created successfully</response>
    /// <response code="400">Invalid input or duplicate category</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var command = new CreateCategoryCommand { Name = dto.Name };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, 
                ApiResponse<CategoryResponseDto>.SuccessResponse(result, "Category created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to create category", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="dto">Category update details</param>
    /// <returns>Updated category details</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/categories/123e4567-e89b-12d3-a456-426614174000
    ///     {
    ///       "name": "Electronics Updated",
    ///       "isActive": true
    ///     }
    /// </remarks>
    /// <response code="200">Category updated successfully</response>
    /// <response code="400">Invalid input or duplicate category</response>
    /// <response code="404">Category not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<CategoryResponseDto>.SuccessResponse(result, "Category updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to update category", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Category deleted successfully</response>
    /// <response code="400">Category has products</response>
    /// <response code="404">Category not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteCategoryCommand { Id = id });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to delete category", new List<string> { ex.Message }));
        }
    }
}
