namespace PaymentService.Models;

public enum PaymentStatus { Pending, Completed, Failed }

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "VietQR";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? QrDataUrl { get; set; }
    public string? VietQrOrderId { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}
