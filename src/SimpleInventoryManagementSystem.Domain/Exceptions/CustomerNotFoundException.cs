namespace SimpleInventoryManagementSystem.Domain.Exceptions;

public sealed class CustomerNotFoundException(Guid customerId)
    : DomainException($"Customer {customerId} was not found");
