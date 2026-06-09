using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using SimpleInventoryManagementSystem.API.Middleware;
using SimpleInventoryManagementSystem.Application.Extensions;
using SimpleInventoryManagementSystem.Infrastructure.Extensions;
using SimpleInventoryManagementSystem.Infrastructure.Persistence;
using SimpleInventoryManagementSystem.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Host.UseSerilog((ctx, cfg) => cfg.WriteTo.Console());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SIMSDbContext>();
    await db.Database.MigrateAsync();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DataSeeder));
    await DataSeeder.SeedAsync(db, seederLogger);
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

app.Run();

public partial class Program { }