namespace SimpleInventoryManagementSystem.Domain.Events;

public record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    int Stock,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
