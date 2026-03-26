using OrderService.Behaviors;
using OrderService.Configurations;
using OrderService.Data;
using OrderService.Messaging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure MongoDB for Read Store
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});
builder.Services.AddScoped<IOrderReadRepository, OrderReadRepository>();
builder.Services.AddHostedService<OrderProjectionSyncService>();

// Configure Kafka Settings
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<OrderKafkaConsumerService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add MediatR with validation behavior
var assembly = Assembly.GetExecutingAssembly();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrate and seed the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database migration for OrderService...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migration completed successfully");

        // Seed sample data
        OrderSeeder.Seed(dbContext);
        logger.LogInformation("Database seeding completed successfully");

        // Initial sync to MongoDB read store
        var readRepository = scope.ServiceProvider.GetRequiredService<IOrderReadRepository>();
        var orders = dbContext.Orders.Include(o => o.OrderItems).ToList();
        if (orders.Count > 0)
        {
            await readRepository.SyncFromWriteStoreAsync(orders);
            logger.LogInformation($"Initial sync completed: {orders.Count} orders synced to MongoDB");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
