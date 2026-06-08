using MediatR;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;

namespace SimpleInventoryManagementSystem.Application.Products.Queries.GetProducts;

public sealed class GetProductsHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var page = await productRepository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = page.Items
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock))
            .ToList();

        return new PagedResult<ProductDto>(items, page.TotalCount, request.PageNumber, request.PageSize);
    }
}
