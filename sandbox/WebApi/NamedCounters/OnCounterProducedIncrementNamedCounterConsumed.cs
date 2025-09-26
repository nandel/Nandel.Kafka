using Microsoft.EntityFrameworkCore;
using Nandel.Kafka.Contracts;
using WebApi.Data;

namespace WebApi.NamedCounters;

[MessageConsumer(CounterProducedEvent.TOPIC_NAME, "c.on.e-counter_produced.increment_named_counter_consumed")]
public class OnCounterProducedIncrementNamedCounterConsumed(WebApiDbContext db) : IMessageHandler<CounterProducedEvent>
{
    public async Task HandleAsync(IMessageEnvelope<CounterProducedEvent> envelope, CounterProducedEvent message, CancellationToken cancel)
    {
        await db.Set<NamedCounter>()
            .Where(counter => counter.Name == message.CounterName)
            .ExecuteUpdateAsync(set => set.SetProperty(counter => counter.ConsumedMessages, counter => counter.ConsumedMessages + 1), cancel);
    }
}