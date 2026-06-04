using SimpleInventoryManagementSystem.Domain.Enums;

namespace SimpleInventoryManagementSystem.Domain.Entities;

public class Customer
{
    private Customer() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Location Location { get; private set; }

    public static Customer Create(string name, string email, Location location)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Location = location
        };
    }
}
