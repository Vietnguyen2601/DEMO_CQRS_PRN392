using MediatR;
using PaymentService.Dtos;

namespace PaymentService.Commands;

public class CreatePaymentCommand : IRequest<PaymentResponseDto>
{
    public Guid OrderId { get; set; }
}

public class UpdatePaymentStatusCommand : IRequest<PaymentResponseDto>
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}
