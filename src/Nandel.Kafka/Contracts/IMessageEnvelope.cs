using System;

namespace Nandel.Kafka.Contracts;

public interface IMessageEnvelope<T>
{
    string Key { get; }
    T Value { get; }
    DateTime TimestampUtc { get; }
}