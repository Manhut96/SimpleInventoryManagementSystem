namespace SimpleInventoryManagementSystem.Domain.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
