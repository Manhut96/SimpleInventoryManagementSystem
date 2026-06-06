using SimpleInventoryManagementSystem.Application.Common;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Domain.Interfaces;

namespace SimpleInventoryManagementSystem.Application.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(
    ITransactionScopeFactory transactionScopeFactory,
    ISIMSDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : TransactionalHandler<CreateProductCommand, ProductDto>(transactionScopeFactory, dbContext, dateTimeProvider)
{
    protected override Task<ProductDto> HandleCoreAsync(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = ProductEntity.Create(request.Name, request.Description, request.Price, request.InitialStock);
        DbContext.Products.Add(product);
        WriteEvent(new ProductCreatedEvent(product.Id, product.Name, product.Price, product.Stock, DateTimeProvider.UtcNow));
        return Task.FromResult(new ProductDto(product.Id, product.Name, product.Description, product.Price, product.Stock));
    }
}
