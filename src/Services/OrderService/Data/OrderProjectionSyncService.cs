using Microsoft.EntityFrameworkCore;

namespace OrderService.Data;

public class OrderProjectionSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderProjectionSyncService> _logger;

    public OrderProjectionSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderProjectionSyncService> logger)
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
                _logger.LogWarning(ex, "Background PostgreSQL->MongoDB projection sync failed");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RunSyncOnce(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var writeDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IOrderReadRepository>();

        var orders = await writeDbContext.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        if (orders.Count > 0)
        {
            await readRepository.SyncFromWriteStoreAsync(orders, stoppingToken);
            _logger.LogDebug("Synced {OrderCount} orders to MongoDB read store", orders.Count);
        }
    }
}
