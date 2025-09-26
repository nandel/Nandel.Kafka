using System.Text.Json.Serialization;
using Nandel.Kafka.Contracts;

namespace WebApi.NamedCounters;

[MessageTopic(TOPIC_NAME)]
public class CounterProducedEvent
{
    public const string TOPIC_NAME = "e.counter_produced";
    
    [JsonPropertyName("id")]
    public Guid Id { get; init; } = Guid.CreateVersion7();
    
    [JsonPropertyName("counter_name")]
    public required string CounterName { get; set; }
    
    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; init; }
}