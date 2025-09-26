using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nandel.Kafka.Outbox.Contracts;
using WebApi.Data;

namespace WebApi.NamedCounters;

public class NamedCounterController(IOutboxMessagePublisher publisher, WebApiDbContext db) : Controller
{
    [Route("/named-counter/{name}"), HttpGet, HttpPost]
    public async Task<IActionResult> Push(string name, CancellationToken cancel)
    {
        var transaction = await db.Database.BeginTransactionAsync(cancel);
        
        var counter = await db.Set<NamedCounter>().FirstOrDefaultAsync(counter => counter.Name == name, cancel);
        if (counter is null)
        {
            counter = new NamedCounter { Name = name };
            await db.Set<NamedCounter>().AddAsync(counter, cancel);
        }

        counter.ProducedMessages++;

        await publisher.PublishAsync(name, new CounterProducedEvent()
        {
            CounterName = name,
            CreatedAt = DateTime.UtcNow
        }, cancel);

        await db.SaveChangesAsync(cancel);
        await transaction.CommitAsync(cancel);

        return Ok(counter);
    }
}