using FluentValidation;

namespace SimpleInventoryManagementSystem.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(q => q.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}
