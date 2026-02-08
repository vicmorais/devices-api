using Asp.Versioning;
using Devices.Api.Middleware;
using Devices.Application.Interfaces;
using Devices.Application.Services;
using Devices.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Service Registration
// ============================================

// MVC Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// OpenAPI document generation (built-in .NET 10)
builder.Services.AddOpenApi();

// API Versioning - enables versioned routes (e.g. /api/v1/devices)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// PostgreSQL database context
builder.Services.AddDbContext<DevicesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IDevicesDbContext resolving to the existing DbContext instance
builder.Services.AddScoped<IDevicesDbContext>(provider =>
    provider.GetRequiredService<DevicesDbContext>());

// Application services
builder.Services.AddScoped<IDeviceService, DeviceService>();

// FluentValidation - auto-register all validators from Application assembly
builder.Services.AddValidatorsFromAssemblyContaining<Devices.Application.Validators.CreateDeviceValidator>();

// Global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ============================================
// Migrations
// ============================================

using(var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

    if(pendingMigrations.Any())
    {
        await dbContext.Database.MigrateAsync();
    }
}

// ============================================
// Middleware Pipeline
// ============================================

app.UseExceptionHandler();

// OpenAPI and Swagger UI (development only)
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Devices API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
