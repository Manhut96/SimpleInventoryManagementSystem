using MediatR;
using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;

namespace SimpleInventoryManagementSystem.Application.Products.Queries.GetProducts;

public sealed class GetProductsHandler(ISIMSDbContext dbContext)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await dbContext.Products.CountAsync(cancellationToken);
        var items = await dbContext.Products
            .AsNoTracking()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
