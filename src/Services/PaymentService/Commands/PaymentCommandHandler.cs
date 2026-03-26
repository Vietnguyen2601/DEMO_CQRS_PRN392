using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Dtos;
using PaymentService.Models;
using PaymentService.Services;

namespace PaymentService.Commands;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentResponseDto>
{
    private readonly PaymentDbContext _dbContext;
    private readonly IVietQrService _vietQrService;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(
        PaymentDbContext dbContext,
        IVietQrService vietQrService,
        IOrderServiceClient orderServiceClient,
        ILogger<CreatePaymentCommandHandler> logger)
    {
        _dbContext = dbContext;
        _vietQrService = vietQrService;
        _orderServiceClient = orderServiceClient;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // Check if payment already exists for this order
        var existing = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.Status != PaymentStatus.Failed, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Payment already exists for order {request.OrderId} with status {existing.Status}");

        // Get order amount from OrderService
        var amount = await _orderServiceClient.GetOrderTotalAmountAsync(request.OrderId, cancellationToken);
        _logger.LogInformation("Fetched order {OrderId} amount: {Amount}", request.OrderId, amount);

        // Create short orderId for VietQR (max 13 chars)
        var vietQrOrderId = Guid.NewGuid().ToString("N")[..13];
        var content = $"TT DH {vietQrOrderId}";

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            Amount = amount,
            Method = "VietQR",
            Status = PaymentStatus.Pending,
            VietQrOrderId = vietQrOrderId,
            CreatedAt = DateTime.UtcNow
        };

        // Call VietQR to generate QR code
        try
        {
            var qrResult = await _vietQrService.GenerateQrCodeAsync(
                amount,
                vietQrOrderId,
                content,
                cancellationToken);

            if (qrResult.Success)
            {
                payment.QrDataUrl = qrResult.QrDataUrl;
                _logger.LogInformation("VietQR code generated for order {OrderId}", request.OrderId);
            }
            else
            {
                _logger.LogWarning("VietQR generation failed: {Error}. Payment created without QR.", qrResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate VietQR code. Payment created without QR.");
        }

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentId} created for order {OrderId}, amount {Amount}", payment.Id, payment.OrderId, amount);

        return MapToDto(payment);
    }

    private static PaymentResponseDto MapToDto(Payment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        Amount = p.Amount,
        Method = p.Method,
        Status = p.Status.ToString(),
        QrDataUrl = p.QrDataUrl,
        CreatedAt = p.CreatedAt,
        PaidAt = p.PaidAt
    };
}

public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand, PaymentResponseDto>
{
    private readonly PaymentDbContext _dbContext;
    private readonly ILogger<UpdatePaymentStatusCommandHandler> _logger;

    public UpdatePaymentStatusCommandHandler(PaymentDbContext dbContext, ILogger<UpdatePaymentStatusCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {request.Id} not found");

        if (Enum.TryParse<PaymentStatus>(request.Status, true, out var status))
        {
            payment.Status = status;
            if (status == PaymentStatus.Completed)
                payment.PaidAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrEmpty(request.TransactionId))
            payment.TransactionId = request.TransactionId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentId} status updated to {Status}", payment.Id, payment.Status);

        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Method = payment.Method,
            Status = payment.Status.ToString(),
            QrDataUrl = payment.QrDataUrl,
            CreatedAt = payment.CreatedAt,
            PaidAt = payment.PaidAt
        };
    }
}
