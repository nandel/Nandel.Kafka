using System.Diagnostics;
using Nandel.Kafka.Contracts;

namespace ThroughputBenchmark;

public class Producer(IMessagePublisher publisher, BenchState state)
{
    public async Task<long> PublishUntilCancelRequestedAsync(CancellationToken cancel)
    {
        while (cancel.IsCancellationRequested == false)
        {
            var batch = Enumerable.Range(0, state.MessagesPerSecond)
                .Select(_ => new Message { Value = Guid.NewGuid() })
                .Select(message =>
                {
                    var task = publisher.PublishAsync(message.Value.ToString(), message, CancellationToken.None);
                    state.IncrementMessagesProduced();
                    return task;
                })
                .ToList();

            await Task.WhenAll(batch);
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
        }

        // This timestamp won't reflect the final state since publishing is async.
        return Stopwatch.GetTimestamp();
    }
}