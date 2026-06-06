namespace SimpleInventoryManagementSystem.Domain.Entities;

public class OutboxEventEntity
{
    private OutboxEventEntity() { }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public static OutboxEventEntity Create(string eventType, string payload, DateTimeOffset createdAt)
    {
        return new OutboxEventEntity
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            CreatedAt = createdAt
        };
    }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
    }
}
