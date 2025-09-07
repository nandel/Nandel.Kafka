using System.Diagnostics;

namespace ThroughputBenchmark.Benchmark;

public class BenchmarkState
{
    public long StartTimestamp { get; private set; }
    public long ElapsedTicks => Stopwatch.GetTimestamp() - StartTimestamp;
    
    public int TargetMessageProductionPerSecond { get; set; } = 80;

    
    public int MessagesProduced => _messagesProduced;
    public int MessagesConsumed => _messagesConsumed;
    
    private int _messagesProduced = 0;
    private int _messagesConsumed = 0;

    public decimal WorkloadCompleted => (decimal) MessagesConsumed / MessagesProduced;
    public double MessagesProducedPerSecond => _messagesProduced / TimeSpan.FromTicks(ElapsedTicks).TotalSeconds;
    public double MessagesConsumedPerSecond => _messagesConsumed / TimeSpan.FromTicks(ElapsedTicks).TotalSeconds;

    public void StartMeasurement()
    {
        StartTimestamp = Stopwatch.GetTimestamp();
        _messagesProduced = 0;
        _messagesConsumed = 0;
    }

    public void IncrementMessagesProduced()
    {
        Interlocked.Increment(ref _messagesProduced);
    }

    public void IncrementMessagesConsumed()
    {
        Interlocked.Increment(ref _messagesConsumed);
    }
}