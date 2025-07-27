using FluentValidation;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Transactions.RegisterTransaction
{
    public class RegisterTransactionCommandValidator : AbstractValidator<RegisterTransactionCommand>
    {
        public RegisterTransactionCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .NotEqual(Guid.Empty);

            RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Must(c => CurrencyService.GetCurrency(c) != "");
        }
    }
}