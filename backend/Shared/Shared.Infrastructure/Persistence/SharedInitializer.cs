using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared.Infrastructure.Persistence;

internal sealed class SharedInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public SharedInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<SharedDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
