using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nandel.Kafka.Contracts;
using Nandel.Kafka.Outbox.Contracts;
using Nandel.Kafka.Outbox.Data;

namespace Nandel.Kafka.Outbox.Services;

public class OutboxDispatcher<TDbContext> : BackgroundService where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessagePublisher _publisher;
    private readonly IClock _systemClock;
    private readonly ILogger<OutboxDispatcher<TDbContext>> _logger;

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, IMessagePublisher publisher, IClock systemClock, ILogger<OutboxDispatcher<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _systemClock = systemClock;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // 💡 Let host continue startup
        
        _logger.LogInformation("🚀 Outbox Dispatcher is starting.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(paginationSize: 80, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Outbox Dispatcher Unhandled Exception");
                await Task.Delay(1000, stoppingToken);
            }
        }
        
        _logger.LogInformation("Outbox Dispatcher is stopping.");
    }

    private async Task RunAsync(long paginationSize, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
            
        var messageList = await db.Set<OutboxMessage>()
            .FromSql(GetSqlQuery(paginationSize))
            .ToListAsync(stoppingToken);

        if (messageList.Count == 0)
        {
            await transaction.RollbackAsync(stoppingToken);
            await Task.Delay(1_000, stoppingToken); // ℹ️ Only "sleep" when there is no messages
            return;
        }
        
        var publishTaskList = messageList
            .Select(message => TryPublishOrSkipAsync(message, stoppingToken))
            .ToList();
    
        await Task.WhenAll(publishTaskList);
        await db.SaveChangesAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);
    }

    private async Task TryPublishOrSkipAsync(OutboxMessage message, CancellationToken stoppingToken)
    {
        try
        {
            await _publisher.PublishAsync(
                topic: message.Topic, 
                key: message.Key, 
                value: message.Value,
                stoppingToken);
                    
            message.SentAt = _systemClock.UtcNow;

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Outbox Message {Uid} published in topic {Topic}, the delay was {Delay:N} ms.",
                    message.Uid, message.Topic, message.SendDelay().TotalMilliseconds);    
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message {Uid}", message.Uid);
        }
    }

    private static FormattableString GetSqlQuery(long paginationSize)
    {
        return $"""
                SELECT * FROM outbox_messages
                WHERE sent_at IS NULL
                ORDER BY created_at
                FOR UPDATE SKIP LOCKED
                LIMIT {paginationSize} 
                """;
    }
}