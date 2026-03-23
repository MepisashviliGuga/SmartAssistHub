using FluentValidation;
using MediatR;
using ValidationException = SmartAssistHub.Application.Common.Exceptions.ValidationException;

namespace SmartAssistHub.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v =>
                v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .GroupBy(
                f => f.PropertyName,
                f => f.ErrorMessage,
                (property, errors) => new
                {
                    Key = property,
                    Values = errors.Distinct().ToArray()
                })
            .ToDictionary(x => x.Key, x => x.Values);

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}