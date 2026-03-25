using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Commands;
using ProductService.Dtos;
using ProductService.Queries;

namespace ProductService.Controllers;

/// <summary>
/// Product Management API Controller
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of all products</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProductResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ProductResponseDto>>>> GetAllProducts()
    {
        try
        {
            var products = await _mediator.Send(new GetAllProductsQuery());
            return Ok(ApiResponse<List<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve products", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    /// <response code="200">Product found</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetProductById(Guid id)
    {
        try
        {
            var product = await _mediator.Send(new GetProductByIdQuery(id));
            return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve product", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get products by category ID
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of products in the category</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ProductResponseDto>>>> GetProductsByCategory(Guid categoryId)
    {
        try
        {
            var products = await _mediator.Send(new GetProductsByCategoryQuery(categoryId));
            return Ok(ApiResponse<List<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve products", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get products by shop ID
    /// </summary>
    /// <param name="shopId">Shop ID</param>
    /// <returns>List of products from the shop</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("shop/{shopId}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ProductResponseDto>>>> GetProductsByShop(Guid shopId)
    {
        try
        {
            var products = await _mediator.Send(new GetProductsByShopQuery(shopId));
            return Ok(ApiResponse<List<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve products", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="dto">Product creation details</param>
    /// <returns>Created product details</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/products
    ///     {
    ///       "shopId": "123e4567-e89b-12d3-a456-426614174000",
    ///       "categoryId": "223e4567-e89b-12d3-a456-426614174000",
    ///       "name": "Laptop Pro",
    ///       "description": "High performance laptop",
    ///       "price": 1500.00,
    ///       "stockQuantity": 50
    ///     }
    /// </remarks>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid input or category not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> CreateProduct([FromBody] CreateProductDto dto)
    {
        try
        {
            var command = new CreateProductCommand
            {
                ShopId = dto.ShopId,
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, 
                ApiResponse<ProductResponseDto>.SuccessResponse(result, "Product created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to create product", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Product update details</param>
    /// <returns>Updated product details</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/products/123e4567-e89b-12d3-a456-426614174000
    ///     {
    ///       "name": "Laptop Pro Max",
    ///       "price": 1800.00,
    ///       "stockQuantity": 30
    ///     }
    /// </remarks>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid input or category not found</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var command = new UpdateProductCommand
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(result, "Product updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to update product", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">New stock quantity</param>
    /// <returns>Updated product details</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PATCH /api/products/123e4567-e89b-12d3-a456-426614174000/stock
    ///     {
    ///       "quantity": 100
    ///     }
    /// </remarks>
    /// <response code="200">Stock updated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("{id}/stock")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> UpdateProductStock(Guid id, [FromBody] UpdateProductStockDto dto)
    {
        try
        {
            var command = new UpdateProductStockCommand
            {
                Id = id,
                Quantity = dto.Quantity
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(result, "Stock updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to update stock", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteProductCommand { Id = id });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to delete product", new List<string> { ex.Message }));
        }
    }
}
