using FluentValidation;
using MediatR;
using ValidationException = FluentValidation.ValidationException;

namespace ModulebankProject.PipelineBehaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);
            var failure = _validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .Take(1).ToList();

            if (failure.Any())
            {
                throw new ValidationException(failure);
            }

            return next(cancellationToken);
        }
    }
}
