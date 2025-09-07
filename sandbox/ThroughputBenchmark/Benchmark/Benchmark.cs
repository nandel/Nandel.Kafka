using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using ThroughputBenchmark.Components;

namespace ThroughputBenchmark.Benchmark;

public class Benchmark(BenchmarkState state, Producer producer, ILogger<Benchmark> logger)
{
    public async Task<NameValueCollection> ExecuteAsync(int testWindowSeconds, CancellationToken stop)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(testWindowSeconds));
        var cancel = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stop).Token;

        state.StartMeasurement();
        var producerTask = producer.PublishUntilCancelRequestedAsync(cancel);

        while (cts.Token.IsCancellationRequested == false)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);

            var elapsed = TimeSpan.FromTicks(state.ElapsedTicks);
            state.TargetMessageProductionPerSecond = state.WorkloadCompleted switch
            {
                > 0.99m => state.TargetMessageProductionPerSecond * 3,
                > 0.98m => Math.Max(2, (int) (state.TargetMessageProductionPerSecond * 2)),
                > 0.97m => Math.Max(2, (int) (state.TargetMessageProductionPerSecond * 1.5)),
                > 0.96m => Math.Max(2, (int) (state.TargetMessageProductionPerSecond * 1.2)),
                > 0.95m => Math.Max(2, (int) (state.TargetMessageProductionPerSecond * 1.1)),
                _ => Math.Max(1, (int) (state.TargetMessageProductionPerSecond * 0.95))
            };

            logger.LogInformation(
                "[{Elapsed:c}] [{MessagesConsumedPerSecond:N} mc/s] [{MessagesProducedPerSecond:N} mp/s] {Percentile:P} workload completed, totals: {MessagesConsumed:N}/{MessagesProduced:N} ",
                elapsed, state.MessagesConsumedPerSecond, state.MessagesProducedPerSecond, state.WorkloadCompleted, state.MessagesConsumed, state.MessagesProduced);
        }

        var producerStopedTimestamp = await producerTask;

        while (state.MessagesConsumed < state.MessagesProduced && stop.IsCancellationRequested == false)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            
            logger.LogInformation(
                "[{Elapsed:c}] [{MessagesConsumedPerSecond:N} mc/s] [{MessagesProducedPerSecond:N} mp/s] {Percentile:P} workload completed, totals: {MessagesConsumed:N}/{MessagesProduced:N} ",
                TimeSpan.FromTicks(state.ElapsedTicks), state.MessagesConsumedPerSecond, state.MessagesProducedPerSecond, state.WorkloadCompleted, state.MessagesConsumed, state.MessagesProduced);
        }

        var endTimestamp = Stopwatch.GetTimestamp();

        return new NameValueCollection()
        {
            { "Execution Length (seconds)", testWindowSeconds.ToString() },
            { "Messages produced", state.MessagesProduced.ToString() },
            { "Messages consumed", state.MessagesConsumed.ToString() },
            { "Messages per second", (state.MessagesConsumed / TimeSpan.FromTicks(endTimestamp - state.StartTimestamp).TotalSeconds).ToString("N") },
            
            { "", "" },
            { "OS", $"{Environment.OSVersion.VersionString}; 64-bit:{Environment.Is64BitOperatingSystem}" },
            { "Number of logical processors", $"{Environment.ProcessorCount}" },
            { ".NET version", $"{Environment.Version}" },
        };
    }
}