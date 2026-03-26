using MediatR;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using OrderService.Commands;
using OrderService.Dtos;
using OrderService.Queries;

namespace OrderService.Controllers;

/// <summary>
/// Orders Management API Controller
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    /// <returns>List of all orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<OrderResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetAllOrders()
    {
        try
        {
            _logger.LogInformation("GetAllOrders endpoint called");
            var orders = await _mediator.Send(new GetAllOrdersQuery());
            return Ok(ApiResponse<List<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve orders", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetOrderById(Guid orderId)
    {
        try
        {
            _logger.LogInformation("GetOrderById called with orderId: {OrderId}", orderId);
            var order = await _mediator.Send(new GetOrderByIdQuery(orderId));
            return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve order", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get orders by account ID
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>List of orders for the account</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetOrdersByAccount(Guid accountId)
    {
        try
        {
            _logger.LogInformation("GetOrdersByAccount called with accountId: {AccountId}", accountId);
            var orders = await _mediator.Send(new GetOrdersByAccountQuery(accountId));
            return Ok(ApiResponse<List<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for account {AccountId}", accountId);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve orders", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="createOrderDto">Order creation data</param>
    /// <returns>Created order details</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResponse("Invalid request data"));

            _logger.LogInformation("CreateOrder called for account {AccountId}", createOrderDto.AccountId);

            var command = new CreateOrderCommand
            {
                AccountId = createOrderDto.AccountId,
                DeliveryAddress = createOrderDto.DeliveryAddress,
                OrderItems = createOrderDto.OrderItems
            };

            var order = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order created successfully"));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error creating order: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            var errorMessages = ex.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid order data", errorMessages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to create order", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="updateOrderDto">Order update data</param>
    /// <returns>Updated order details</returns>
    /// <response code="200">Order updated successfully</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{orderId}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateOrder(Guid orderId, [FromBody] UpdateOrderDto updateOrderDto)
    {
        try
        {
            _logger.LogInformation("UpdateOrder called for orderId: {OrderId}", orderId);

            var command = new UpdateOrderCommand
            {
                OrderId = orderId,
                DeliveryAddress = updateOrderDto.DeliveryAddress
            };

            var order = await _mediator.Send(command);
            return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order updated successfully"));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error updating order: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            var errorMessages = ex.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid order data", errorMessages));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to update order", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Success status</returns>
    /// <response code="200">Order deleted successfully</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{orderId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteOrder(Guid orderId)
    {
        try
        {
            _logger.LogInformation("DeleteOrder called for orderId: {OrderId}", orderId);

            var command = new DeleteOrderCommand { OrderId = orderId };
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Order deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to delete order", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get order items by order ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>List of order items</returns>
    /// <response code="200">Items retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{orderId}/items")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderItemResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<OrderItemResponseDto>>>> GetOrderItems(Guid orderId)
    {
        try
        {
            _logger.LogInformation("GetOrderItems called for orderId: {OrderId}", orderId);
            var items = await _mediator.Send(new GetOrderItemsByOrderIdQuery(orderId));
            return Ok(ApiResponse<List<OrderItemResponseDto>>.SuccessResponse(items, "Order items retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items for order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve order items", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Add items to an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="itemsDto">Items to add</param>
    /// <returns>Updated order details</returns>
    /// <response code="200">Items added successfully</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{orderId}/items")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> AddOrderItems(Guid orderId, [FromBody] List<CreateOrderItemDto> itemsDto)
    {
        try
        {
            _logger.LogInformation("AddOrderItems called for orderId: {OrderId}", orderId);

            var command = new AddOrderItemCommand
            {
                OrderId = orderId,
                OrderItems = itemsDto
            };

            var order = await _mediator.Send(command);
            return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Items added successfully"));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error adding items: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            var errorMessages = ex.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid item data", errorMessages));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding items to order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to add items", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Remove an item from order
    /// </summary>
    /// <param name="orderItemId">Order Item ID</param>
    /// <returns>Success status</returns>
    /// <response code="200">Item removed successfully</response>
    /// <response code="404">Item not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("items/{orderItemId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveOrderItem(Guid orderItemId)
    {
        try
        {
            _logger.LogInformation("RemoveOrderItem called for orderItemId: {OrderItemId}", orderItemId);

            var command = new RemoveOrderItemCommand { OrderItemId = orderItemId };
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Order item removed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order item not found: {OrderItemId}", orderItemId);
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing order item {OrderItemId}", orderItemId);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to remove order item", new List<string> { ex.Message }));
        }
    }
}
