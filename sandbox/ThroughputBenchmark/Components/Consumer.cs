using Nandel.Kafka.Contracts;
using ThroughputBenchmark.Benchmark;

namespace ThroughputBenchmark.Components;

[MessageConsumer(Message.TopicName, "benchs.throughput.consumer")]
public class Consumer(ILogger<Consumer> logger, BenchmarkState state) : IMessageHandler<Message>
{
    public Task HandleAsync(IMessageEnvelope<Message> envelope, Message message, CancellationToken cancel)
    {
        state.IncrementMessagesConsumed();
        return Task.CompletedTask;
    }
}