using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentService.Services;

/// <summary>
/// Client to call OrderService API to get order details
/// </summary>
public interface IOrderServiceClient
{
    Task<decimal> GetOrderTotalAmountAsync(Guid orderId, CancellationToken cancellationToken);
}

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderServiceClient> _logger;

    public OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetOrderTotalAmountAsync(Guid orderId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching order {OrderId} from OrderService", orderId);

        var response = await _httpClient.GetAsync($"/api/orders/{orderId}", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("OrderService response: {StatusCode}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Order {orderId} not found in OrderService. Status: {response.StatusCode}");

        var apiResponse = JsonSerializer.Deserialize<OrderApiResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (apiResponse?.Data == null)
            throw new InvalidOperationException($"Invalid response from OrderService for order {orderId}");

        return (decimal)apiResponse.Data.TotalAmount;
    }
}

// DTOs to deserialize OrderService response
public class OrderApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public OrderData? Data { get; set; }
}

public class OrderData
{
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public double TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
}
