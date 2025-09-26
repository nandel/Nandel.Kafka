using System.Text.Json.Serialization;

namespace Nandel.Kafka.Outbox.Data;

public class OutboxMessage
{
    [JsonPropertyName("_uid")]
    public Guid Uid { get; init; } = Guid.CreateVersion7();
    
    [JsonPropertyName("topic")]
    public required string Topic { get; init; }
    
    [JsonPropertyName("key")]
    public required string Key { get; init; }
    
    [JsonPropertyName("value")]
    public required string Value { get; init; }
    
    [JsonPropertyName("sent_at")]
    public DateTime? SentAt { get; set; }
    
    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; init; }
    
    public TimeSpan SendDelay()
    {
        if (!SentAt.HasValue) throw new InvalidOperationException("Can't calculated Elapsed Time for Outbox Message because it was not processed.");
        return (SentAt.Value - CreatedAt);
    }
}