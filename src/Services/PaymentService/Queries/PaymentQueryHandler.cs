using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Dtos;

namespace PaymentService.Queries;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentResponseDto>
{
    private readonly PaymentDbContext _dbContext;

    public GetPaymentByIdQueryHandler(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentResponseDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {request.Id} not found");

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

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentResponseDto>
{
    private readonly PaymentDbContext _dbContext;

    public GetPaymentByOrderIdQueryHandler(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentResponseDto> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.OrderId == request.OrderId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Payment for order {request.OrderId} not found");

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

public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, List<PaymentResponseDto>>
{
    private readonly PaymentDbContext _dbContext;

    public GetAllPaymentsQueryHandler(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PaymentResponseDto>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status.ToString(),
                QrDataUrl = p.QrDataUrl,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt
            })
            .ToListAsync(cancellationToken);
    }
}
