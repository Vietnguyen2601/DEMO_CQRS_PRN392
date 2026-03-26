namespace PaymentService.Configurations;

public class VietQrSettings
{
    public string BaseUrl { get; set; } = "https://api.vietqr.org";
    public string ApiUsername { get; set; } = string.Empty;
    public string ApiPassword { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}
