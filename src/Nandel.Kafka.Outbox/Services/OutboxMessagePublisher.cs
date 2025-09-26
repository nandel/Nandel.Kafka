using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nandel.Kafka.Outbox.Contracts;
using Nandel.Kafka.Outbox.Data;

namespace Nandel.Kafka.Outbox.Services;

public class OutboxMessagePublisher<TDbContext>(TDbContext db, IClock clock, ILogger<OutboxMessagePublisher<TDbContext>> logger) : IOutboxMessagePublisher where TDbContext : DbContext
{
    private readonly DbSet<OutboxMessage> _set = db.Set<OutboxMessage>();
    
    public async Task PublishAsync(string topic, string key, string value, CancellationToken cancel)
    {
        if (db.Database.CurrentTransaction is null)
        {
            logger.LogWarning("A message {Key} to the topic {Topic} has been created with no current transaction.", key, topic);
        }
        
        var outboxMessage = new OutboxMessage
        {
            Topic = topic,
            Key = key,
            Value = value,
            CreatedAt = clock.UtcNow,
        };
        
        await _set.AddAsync(outboxMessage, cancel);
    }
}