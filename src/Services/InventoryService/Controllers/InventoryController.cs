using InventoryService.Commands;
using InventoryService.Dtos;
using InventoryService.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<InventoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<InventoryResponseDto>>>> GetAllInventoryItems()
    {
        try
        {
            var items = await _mediator.Send(new GetAllInventoryItemsQuery());
            return Ok(ApiResponse<List<InventoryResponseDto>>.SuccessResponse(items, "Inventory items retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve inventory items", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryResponseDto>>> GetInventoryItemById(Guid id)
    {
        try
        {
            var item = await _mediator.Send(new GetInventoryItemByIdQuery(id));
            return Ok(ApiResponse<InventoryResponseDto>.SuccessResponse(item, "Inventory item retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve inventory item", new List<string> { ex.Message }));
        }
    }

    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryResponseDto>>> GetInventoryByProductId(Guid productId)
    {
        try
        {
            var item = await _mediator.Send(new GetInventoryByProductIdQuery(productId));
            return Ok(ApiResponse<InventoryResponseDto>.SuccessResponse(item, "Inventory item retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve inventory item", new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InventoryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryResponseDto>>> CreateInventoryItem([FromBody] CreateInventoryItemDto dto)
    {
        try
        {
            var command = new CreateInventoryItemCommand
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetInventoryItemById), new { id = result.Id }, ApiResponse<InventoryResponseDto>.SuccessResponse(result, "Inventory item created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to create inventory item", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryResponseDto>>> UpdateInventoryItem(Guid id, [FromBody] UpdateInventoryItemDto dto)
    {
        try
        {
            var command = new UpdateInventoryItemCommand
            {
                Id = id,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<InventoryResponseDto>.SuccessResponse(result, "Inventory item updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to update inventory item", new List<string> { ex.Message }));
        }
    }

    [HttpPatch("{id}/quantity")]
    [ProducesResponseType(typeof(ApiResponse<InventoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryResponseDto>>> UpdateInventoryQuantity(Guid id, [FromBody] UpdateInventoryQuantityDto dto)
    {
        try
        {
            var command = new UpdateInventoryQuantityCommand
            {
                Id = id,
                Quantity = dto.Quantity
            };

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<InventoryResponseDto>.SuccessResponse(result, "Inventory quantity updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to update inventory quantity", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInventoryItem(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteInventoryItemCommand { Id = id });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to delete inventory item", new List<string> { ex.Message }));
        }
    }
}
