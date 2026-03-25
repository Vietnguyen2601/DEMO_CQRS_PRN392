using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data;

public class InventoryProjectionSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InventoryProjectionSyncService> _logger;

    public InventoryProjectionSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<InventoryProjectionSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSyncOnce(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background SQL->Mongo projection sync failed");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RunSyncOnce(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IInventoryReadRepository>();

        var writeItems = await dbContext.InventoryItems.AsNoTracking().ToListAsync(cancellationToken);
        await readRepository.SyncFromWriteStoreAsync(writeItems, cancellationToken);
    }
}