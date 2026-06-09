using FluentValidation;
using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Common;

/// <summary>
/// MediatR pipeline behaviour that runs FluentValidation validators before
/// the command handler executes.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            // Surface the first validation failure as a Result failure
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var error = new Error("Validation.Failed", errorMessage);

            // Construct a failed Result<T> or Result via reflection
            var responseType = typeof(TResponse);
            if (responseType == typeof(Result))
                return (TResponse)(object)Result.Failure(error);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = responseType.GetMethod(nameof(Result<object>.Failure))!;
                return (TResponse)failureMethod.Invoke(null, [error])!;
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
