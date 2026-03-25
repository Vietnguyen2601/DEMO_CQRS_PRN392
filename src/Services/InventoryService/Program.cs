using InventoryService.Configurations;
using InventoryService.Data;
using InventoryService.Messaging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});
builder.Services.AddScoped<IInventoryReadRepository, InventoryReadRepository>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<InventoryProjectionSyncService>();
builder.Services.AddHostedService<InventoryKafkaConsumerService>();

var assembly = Assembly.GetExecutingAssembly();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Inventory Service API",
        Version = "v1",
        Description = "API for managing inventory items"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var readRepository = scope.ServiceProvider.GetRequiredService<IInventoryReadRepository>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    dbContext.Database.Migrate();
    InventorySeeder.Seed(dbContext);

    try
    {
        var writeItems = await dbContext.InventoryItems.AsNoTracking().ToListAsync();
        await readRepository.SyncFromWriteStoreAsync(writeItems, CancellationToken.None);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Startup sync to Mongo read store failed. Write side remains available.");
    }
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service API v1");
        c.RoutePrefix = ""; // Set Swagger UI at app's root
    });
}

app.MapControllers();

app.Run();
