using FluentValidation;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Accounts.EditAccount
{
    public class EditAccountCommandValidator : AbstractValidator<EditAccountCommand>
    {
        public EditAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(Guid.Empty);

            RuleFor(x => x.Сurrency)
                .Must(c => CurrencyService.GetCurrency(c) != "");
        }
    }
}