using System.Diagnostics;
using Nandel.Kafka.Contracts;
using ThroughputBenchmark.Benchmark;

namespace ThroughputBenchmark.Components;

public class Producer(IMessagePublisher publisher, BenchmarkState state)
{
    public async Task<long> PublishUntilCancelRequestedAsync(CancellationToken cancel)
    {
        while (cancel.IsCancellationRequested == false)
        {
            var batch = Enumerable.Range(0, state.TargetMessageProductionPerSecond)
                .Select(_ => new MessageBody { Value = Guid.NewGuid() })
                .Select(message => publisher.PublishAsync(message.Value.ToString(), message, CancellationToken.None).ContinueWith(_ => state.IncrementMessagesProduced(), CancellationToken.None))
                .Union([Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None)])
                .ToList();

            await Task.WhenAll(batch);
        }

        // This timestamp won't reflect the final state since publishing is async.
        return Stopwatch.GetTimestamp();
    }
}