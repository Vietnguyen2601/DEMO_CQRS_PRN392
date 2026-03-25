namespace InventoryService.Messaging;

public class KafkaEventMessage
{
    public string EventType { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}
