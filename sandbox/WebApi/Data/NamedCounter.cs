namespace WebApi.Data;

public class NamedCounter
{
    public required string Name { get; init; }
    public int ProducedMessages { get; set; }
    public int ConsumedMessages { get; set; }
}