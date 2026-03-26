namespace PaymentService.Dtos;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? QrDataUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class CreatePaymentDto
{
    public Guid OrderId { get; set; }
}
