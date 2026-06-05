using MediatR;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Events;

namespace SimpleInventoryManagementSystem.Application.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(ISIMSDbContext dbContext, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.InitialStock);
        await PersistProductAsync(product, cancellationToken);
        return new ProductDto(product.Id, product.Name, product.Description, product.Price, product.Stock);
    }

    private async Task PersistProductAsync(Product product, CancellationToken cancellationToken)
    {
        dbContext.Products.Add(product);
        EnqueueProductCreatedEvent(product);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private void EnqueueProductCreatedEvent(Product product)
        => unitOfWork.Enqueue(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Price,
            product.Stock,
            DateTimeOffset.UtcNow));
}
