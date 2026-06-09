using FluentValidation;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;

namespace SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.Products)
            .NotEmpty();

        RuleForEach(x => x.Products)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty();

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0);
            });
    }
}
