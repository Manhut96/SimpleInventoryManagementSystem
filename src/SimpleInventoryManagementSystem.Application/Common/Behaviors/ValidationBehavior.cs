using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SimpleInventoryManagementSystem.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var failures = await CollectFailuresAsync(request, cancellationToken);

        if (failures.Count != 0)
        {
            logger.LogWarning("Validation failed for {Request}: {Errors}",
                typeof(TRequest).Name,
                string.Join("; ", failures.Select(f => f.ErrorMessage)));
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }

    private async Task<List<FluentValidation.Results.ValidationFailure>> CollectFailuresAsync(TRequest request, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        return results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();
    }
}
