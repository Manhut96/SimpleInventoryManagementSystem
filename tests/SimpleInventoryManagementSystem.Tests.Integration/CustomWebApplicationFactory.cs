using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using Testcontainers.PostgreSql;

namespace SimpleInventoryManagementSystem.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // A neutral date: no Black Friday, no Polish holiday, no discount applies by default.
    public static readonly DateTimeOffset FixedUtcNow = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    async Task IAsyncLifetime.InitializeAsync() => await _postgresContainer.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _postgresContainer.GetConnectionString());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(FixedUtcNow));
        });
    }

    private sealed class FixedDateTimeProvider(DateTimeOffset fixedDate) : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => fixedDate;
    }
}
