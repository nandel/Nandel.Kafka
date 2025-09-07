using System.Collections.Specialized;
using System.Diagnostics;

namespace ThroughputBenchmark;

public class Bench(BenchState state, Producer producer, ILogger<Bench> logger)
{
    public async Task<NameValueCollection> ExecuteAsync(int testWindowSeconds, CancellationToken stopRequested)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(testWindowSeconds));
        var cancel = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stopRequested).Token;

        var startTimestamp = Stopwatch.GetTimestamp();
        var producerTask = producer.PublishUntilCancelRequestedAsync(cancel);

        while (cts.Token.IsCancellationRequested == false)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);

            var elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - startTimestamp);
            var percentile = ((decimal)state.MessagesConsumed / state.MessagesProduced);
            state.MessagesPerSecond = percentile switch
            {
                > 0.95m => state.MessagesPerSecond * 2,
                _ => Math.Max((int) (state.MessagesPerSecond * 0.8), 1)
            };

            logger.LogInformation(
                "[{Elapsed}] {Percentile} Messages Consumed of {MessagesConsumed}/{MessagesProduced} with {MessagesPerSecond} m/s",
                elapsed.ToString("c"),  percentile.ToString("P"), state.MessagesConsumed, state.MessagesProduced, state.MessagesPerSecond);
        }

        var producerStopedTimestamp = await producerTask;

        while (state.MessagesConsumed < state.MessagesProduced && cancel.IsCancellationRequested == false)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            
            logger.LogInformation(
                "[{Elapsed}] {Percentile} Messages Consumed of {MessagesConsumed}/{MessagesProduced} with {MessagesPerSecond} m/s",
                TimeSpan.FromTicks(Stopwatch.GetTimestamp() - startTimestamp).ToString("c"),  ((decimal)state.MessagesConsumed / state.MessagesProduced).ToString("P"), state.MessagesConsumed, state.MessagesProduced, state.MessagesPerSecond);
        }

        var endTimestamp = Stopwatch.GetTimestamp();

        return new NameValueCollection()
        {
            { nameof(testWindowSeconds), testWindowSeconds.ToString() },
            { nameof(startTimestamp), startTimestamp.ToString() },
            { nameof(endTimestamp), endTimestamp.ToString() },
            { nameof(producerStopedTimestamp), producerStopedTimestamp.ToString() },
            { nameof(state.MessagesProduced), state.MessagesProduced.ToString() },
            { nameof(state.MessagesConsumed), state.MessagesConsumed.ToString() },
            { "ThroughputPerSecond", (state.MessagesProduced / testWindowSeconds).ToString() }
        };
    }
}