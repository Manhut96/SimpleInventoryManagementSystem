using SimpleInventoryManagementSystem.Domain.Enums;

namespace SimpleInventoryManagementSystem.Domain.Entities;

public class CustomerEntity
{
    private CustomerEntity() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Location Location { get; private set; }

    public static CustomerEntity Create(string name, string email, Location location, Guid? id = null)
    {
        return new CustomerEntity
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Email = email,
            Location = location
        };
    }
}
