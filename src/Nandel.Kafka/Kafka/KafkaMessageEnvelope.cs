using System;
using System.Text.Json;
using Confluent.Kafka;
using Nandel.Kafka.Contracts;

namespace Nandel.Kafka.Kafka;

public class KafkaMessageEnvelope<T> : IMessageEnvelope<T>
{
    public string Key { get; }
    public T Value { get; }
    public DateTime TimestampUtc { get; }
    public ConsumeResult<string, string> ConsumeResult { get; }

    public KafkaMessageEnvelope(ConsumeResult<string, string> result)
    {
        Key = result.Message.Key;
        Value = JsonSerializer.Deserialize<T>(result.Message.Value) ?? throw new NullReferenceException();
        TimestampUtc = result.Message.Timestamp.UtcDateTime;
        ConsumeResult = result;
    }
}