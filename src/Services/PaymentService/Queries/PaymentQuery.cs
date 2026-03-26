using MediatR;
using PaymentService.Dtos;

namespace PaymentService.Queries;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentResponseDto>;
public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentResponseDto>;
public record GetAllPaymentsQuery() : IRequest<List<PaymentResponseDto>>;
