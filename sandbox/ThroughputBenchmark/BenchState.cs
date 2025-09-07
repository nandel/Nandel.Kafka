namespace ThroughputBenchmark;

public class BenchState
{
    public int MessagesPerSecond { get; set; } = 1;
    
    public int MessagesProduced => _messagesProduced;
    public int MessagesConsumed => _messagesConsumed;

    private int _messagesProduced = 0;
    private int _messagesConsumed = 0;

    public void IncrementMessagesProduced()
    {
        Interlocked.Increment(ref _messagesProduced);
    }

    public void IncrementMessagesConsumed()
    {
        Interlocked.Increment(ref _messagesConsumed);
    }
}