using Microsoft.Extensions.Logging;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Events;

namespace SimpleInventoryManagementSystem.Infrastructure.Services;

// Replace with RabbitMQ or Azure Service Bus client in production
public sealed class MockEventPublisher(ILogger<MockEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Event published (mock): {EventType} {@Event}", domainEvent.GetType().Name, domainEvent);
        return Task.CompletedTask;
    }
}
