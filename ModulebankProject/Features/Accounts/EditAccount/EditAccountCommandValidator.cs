using FluentValidation;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Accounts.EditAccount
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class EditAccountCommandValidator : AbstractValidator<EditAccountCommand>
    {
        public EditAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(Guid.Empty)
                .WithMessage("The Id cannot be empty");

            RuleFor(x => x.Currency)
                .Must(c => CurrencyService.GetCurrency(c) != "")
                .WithMessage("The Currency must be one of the list of currencies");
        }
    }
}