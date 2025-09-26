using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Services;

public class EnsureDatabaseMigrated(IServiceScopeFactory scopeFactory) : IHostedService
{
    private readonly IServiceScope _scope = scopeFactory.CreateScope();
    
    private CancellationTokenSource? _cts;
    private Task? _task;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts ??= CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _task ??= _scope.ServiceProvider.GetRequiredService<WebApiDbContext>()
            .Database.MigrateAsync(_cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is null) return;
        await _cts.CancelAsync();
    }
}