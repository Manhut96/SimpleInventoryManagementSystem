using MediatR;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;

namespace SimpleInventoryManagementSystem.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(string Name, string Description, decimal Price, int InitialStock) : IRequest<ProductDto>;
