using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.NamedCounters;
using WebApi.Services;

namespace WebApi;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddHostedService<EnsureDatabaseMigrated>();
    }
    
    public static void AddConsumers(this IServiceCollection services)
    {
        services.AddKafkaMessageConsumer<CounterProducedEvent, OnCounterProducedIncrementNamedCounterConsumed>();
    }

    public static void AddData(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<WebApiDbContext>(builder =>
        {
            builder.UseNpgsql(config.GetConnectionString("Postgres"));
        });
    }
}