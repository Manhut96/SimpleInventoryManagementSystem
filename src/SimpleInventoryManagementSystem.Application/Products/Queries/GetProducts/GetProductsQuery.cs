using MediatR;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;

namespace SimpleInventoryManagementSystem.Application.Products.Queries.GetProducts;

public record GetProductsQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<ProductDto>>;
