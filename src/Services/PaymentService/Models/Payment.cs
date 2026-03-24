namespace PaymentService.Models;

public enum PaymentStatus { Pending, Completed, Failed }

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "CreditCard"; // e.g. CreditCard, BankTransfer
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
