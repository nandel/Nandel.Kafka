using Nandel.Kafka.Contracts;

namespace ThroughputBenchmark;

[MessageConsumer(Message.TopicName, "benchs.throughput.consumer")]
public class Consumer(ILogger<Consumer> logger, BenchState state) : IMessageHandler<Message>
{
    public Task HandleAsync(IMessageEnvelope<Message> envelope, Message message, CancellationToken cancel)
    {
        state.IncrementMessagesConsumed();
        return Task.CompletedTask;
    }
}