using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nandel.Kafka.Outbox.Contracts;
using Nandel.Kafka.Outbox.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    /// <summary>
    /// Set up the Nandel.Kafka to have an outbox so you can bind the event and the database storage operation
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TDbContext">This is your implementation of a DbContext where you added the OutboxMessage entity.</typeparam>
    /// <returns>IServiceCollection for chaining calls</returns>
    public static IServiceCollection AddNandelKafkaOutbox<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.TryAddSingleton<IClock>(new SystemClock());
        
        services.AddHostedService<OutboxDispatcher<TDbContext>>();
        services.TryAddScoped<IOutboxMessagePublisher, OutboxMessagePublisher<TDbContext>>();
        
        return services;
    }
}