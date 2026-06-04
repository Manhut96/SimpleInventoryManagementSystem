namespace SimpleInventoryManagementSystem.Domain.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
