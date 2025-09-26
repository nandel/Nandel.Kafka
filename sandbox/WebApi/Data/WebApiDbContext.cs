using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class WebApiDbContext(DbContextOptions<WebApiDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebApiDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Nandel.Kafka.Outbox.Data.OutboxMessage).Assembly);
    }
}