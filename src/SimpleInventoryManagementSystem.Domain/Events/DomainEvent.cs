namespace SimpleInventoryManagementSystem.Domain.Events;

public abstract record DomainEvent(DateTimeOffset OccurredAt);
