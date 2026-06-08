using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Products.Models;

public record ProductsPage(int TotalCount, IReadOnlyList<ProductEntity> Items);
