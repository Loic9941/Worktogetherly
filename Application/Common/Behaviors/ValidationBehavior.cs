using ErrorOr;
using FluentValidation;
using MediatR;

namespace WorkTogetherly.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IErrorOr
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var errors = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
                .ToList();

            if (errors.Count != 0)
                // ErrorOr<T> has an implicit conversion from List<Error>; dynamic is the only way to call it generically.
                return (TResponse)(dynamic)errors;

            return await next();
        }
    }
}
