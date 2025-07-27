using FluentValidation;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Accounts.CreateAccount
{
    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountCommandValidator()
        {
            RuleFor(x => x.OwnerId)
                .NotEmpty();

            RuleFor(x => x.Сurrency)
                .NotEmpty()
                .Must(c => CurrencyService.GetCurrency(c) != "");

            RuleFor(x => x.InterestRate)
                .NotEmpty();

            RuleFor(x => x.CloseDate)
                .NotEmpty();
        }
    }
}