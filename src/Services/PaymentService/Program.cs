using Microsoft.EntityFrameworkCore;
using PaymentService.Configurations;
using PaymentService.Data;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// VietQR settings
builder.Services.Configure<VietQrSettings>(builder.Configuration.GetSection("VietQr"));
builder.Services.AddHttpClient<IVietQrService, VietQrService>();
builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5003");
});

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto migrate + create database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
