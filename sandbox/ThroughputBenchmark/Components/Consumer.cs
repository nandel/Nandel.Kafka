using Nandel.Kafka.Contracts;
using ThroughputBenchmark.Benchmark;

namespace ThroughputBenchmark.Components;

[MessageConsumer(MessageBody.TopicName, "benchs.throughput.consumer")]
public class Consumer(ILogger<Consumer> logger, BenchmarkState state) : IMessageHandler<MessageBody>
{
    public Task HandleAsync(IMessageEnvelope<MessageBody> envelope, MessageBody messageBody, CancellationToken cancel)
    {
        state.IncrementMessagesConsumed();
        return Task.CompletedTask;
    }
}