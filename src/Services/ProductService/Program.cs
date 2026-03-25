using ProductService.Configurations;
using ProductService.Data;
using ProductService.Messaging;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});
builder.Services.AddScoped<IProductReadRepository, ProductReadRepository>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<ProductProjectionSyncService>();
builder.Services.AddHostedService<ProductKafkaConsumerService>();

// Add MediatR
var assembly = Assembly.GetExecutingAssembly();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product Service API",
        Version = "v1",
        Description = "API for managing products and categories"
    });
});

var app = builder.Build();

// Migrate and seed the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    var readRepository = scope.ServiceProvider.GetRequiredService<IProductReadRepository>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    dbContext.Database.Migrate();
    ProductSeeder.Seed(dbContext);

    try
    {
        var categories = await dbContext.Categories.AsNoTracking().ToListAsync();
        var products = await dbContext.Products.AsNoTracking().ToListAsync();
        await readRepository.SyncFromWriteStoreAsync(products, categories, CancellationToken.None);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Startup sync to Mongo read store failed. Write side remains available.");
    }
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
    });
}

app.MapControllers();

app.Run();
