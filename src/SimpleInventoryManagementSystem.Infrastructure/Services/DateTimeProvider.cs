using SimpleInventoryManagementSystem.Domain.Interfaces;

namespace SimpleInventoryManagementSystem.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
