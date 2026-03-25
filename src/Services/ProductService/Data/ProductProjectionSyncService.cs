using Microsoft.EntityFrameworkCore;

namespace ProductService.Data;

public class ProductProjectionSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductProjectionSyncService> _logger;

    public ProductProjectionSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<ProductProjectionSyncService> logger)
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
                await SyncOnce(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background SQL->Mongo sync failed for ProductService");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task SyncOnce(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IProductReadRepository>();

        var categories = await dbContext.Categories.AsNoTracking().ToListAsync(cancellationToken);
        var products = await dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);

        await readRepository.SyncFromWriteStoreAsync(products, categories, cancellationToken);
    }
}