using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Commands;
using PaymentService.Dtos;
using PaymentService.Queries;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create payment and generate VietQR code
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        try
        {
            _logger.LogInformation("CreatePayment called for order {OrderId}", dto.OrderId);

            var command = new CreatePaymentCommand
            {
                OrderId = dto.OrderId
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPaymentById), new { id = result.Id },
                ApiResponse<PaymentResponseDto>.SuccessResponse(result, "Payment created. QR code generated."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to create payment", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> GetPaymentById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetPaymentByIdQuery(id));
            return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(result, "Payment retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve payment", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get payment by Order ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> GetPaymentByOrderId(Guid orderId)
    {
        try
        {
            var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId));
            return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(result, "Payment retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve payment", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PaymentResponseDto>>>> GetAllPayments()
    {
        try
        {
            var result = await _mediator.Send(new GetAllPaymentsQuery());
            return Ok(ApiResponse<List<PaymentResponseDto>>.SuccessResponse(result, "Payments retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to retrieve payments", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// VietQR transaction sync callback
    /// </summary>
    [HttpPost("callback/vietqr")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> VietQrCallback([FromBody] VietQrCallbackDto dto)
    {
        try
        {
            _logger.LogInformation("VietQR callback received: orderId={OrderId}, amount={Amount}, transactionId={TransactionId}",
                dto.OrderId, dto.Amount, dto.TransactionId);

            // Find payment by VietQR orderId
            var payments = await _mediator.Send(new GetAllPaymentsQuery());
            var payment = payments.FirstOrDefault(p => p.Id.ToString("N").StartsWith(dto.OrderId ?? ""));

            // Try to find by the VietQrOrderId stored in DB
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for VietQR orderId: {OrderId}", dto.OrderId);
                return Ok(new { code = "00", message = "success" });
            }

            await _mediator.Send(new UpdatePaymentStatusCommand
            {
                Id = payment.Id,
                Status = "Completed",
                TransactionId = dto.TransactionId
            });

            _logger.LogInformation("Payment {PaymentId} marked as Completed via VietQR callback", payment.Id);
            return Ok(new { code = "00", message = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VietQR callback");
            return Ok(new { code = "01", message = "error" });
        }
    }
}

public class VietQrCallbackDto
{
    public string? BankAccount { get; set; }
    public long? Amount { get; set; }
    public string? TransType { get; set; }
    public string? Content { get; set; }
    public string? TransactionId { get; set; }
    public long? TransactionTime { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? OrderId { get; set; }
}
