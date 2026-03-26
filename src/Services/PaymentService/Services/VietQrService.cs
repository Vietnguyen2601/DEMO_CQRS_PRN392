using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PaymentService.Configurations;

namespace PaymentService.Services;

public interface IVietQrService
{
    Task<VietQrGenerateResult> GenerateQrCodeAsync(decimal amount, string orderId, string content, CancellationToken cancellationToken);
}

public class VietQrService : IVietQrService
{
    private readonly HttpClient _httpClient;
    private readonly VietQrSettings _settings;
    private readonly ILogger<VietQrService> _logger;

    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    public VietQrService(HttpClient httpClient, IOptions<VietQrSettings> options, ILogger<VietQrService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<VietQrGenerateResult> GenerateQrCodeAsync(decimal amount, string orderId, string content, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);

        var requestBody = new
        {
            bankCode = _settings.BankCode,
            bankAccount = _settings.AccountNo,
            userBankName = _settings.AccountName,
            content = content.Length > 23 ? content[..23] : content,
            qrType = 0,
            amount = (long)amount,
            orderId = orderId.Length > 13 ? orderId[..13] : orderId,
            transType = "C"
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/vqr/api/qr/generate-customer")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _logger.LogInformation("Calling VietQR Generate QR for orderId: {OrderId}, amount: {Amount}", orderId, amount);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("VietQR Generate QR response: {StatusCode} - {Content}", response.StatusCode, responseContent);

        if (!response.IsSuccessStatusCode)
        {
            return new VietQrGenerateResult
            {
                Success = false,
                ErrorMessage = $"VietQR API error: {response.StatusCode} - {responseContent}"
            };
        }

        try
        {
            var result = JsonSerializer.Deserialize<VietQrApiResponse>(responseContent);
            if (result != null && result.Code == "00")
            {
                return new VietQrGenerateResult
                {
                    Success = true,
                    QrDataUrl = result.Data?.QrDataUrl ?? result.Data?.QrCode ?? string.Empty
                };
            }

            return new VietQrGenerateResult
            {
                Success = false,
                ErrorMessage = result?.Desc ?? "Unknown VietQR error"
            };
        }
        catch (JsonException)
        {
            // Response might be the QR image directly or raw data
            return new VietQrGenerateResult
            {
                Success = true,
                QrDataUrl = responseContent
            };
        }
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiresAt)
            return _cachedToken;

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ApiUsername}:{_settings.ApiPassword}"));

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/vqr/api/token_generate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        _logger.LogInformation("Requesting VietQR token...");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("VietQR token response: {StatusCode} - {Content}", response.StatusCode, content);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to get VietQR token: {response.StatusCode} - {content}");

        var tokenResponse = JsonSerializer.Deserialize<VietQrTokenResponse>(content)
            ?? throw new InvalidOperationException("Invalid VietQR token response");

        _cachedToken = tokenResponse.AccessToken;
        _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30); // 30s buffer

        return _cachedToken;
    }
}

public class VietQrGenerateResult
{
    public bool Success { get; set; }
    public string QrDataUrl { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class VietQrTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class VietQrApiResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public VietQrData? Data { get; set; }
}

public class VietQrData
{
    [JsonPropertyName("qrDataURL")]
    public string? QrDataUrl { get; set; }

    [JsonPropertyName("qrCode")]
    public string? QrCode { get; set; }
}
