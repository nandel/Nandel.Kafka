using System.Text.Json.Serialization;
using Nandel.Kafka.Contracts;

namespace ThroughputBenchmark;

[MessageTopic(TopicName)]
public class Message
{
    public const string TopicName = "benchs.throughput.message";

    [JsonPropertyName("value")] public Guid Value { get; set; }
}