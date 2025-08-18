using FluentValidation;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Accounts.CreateAccount;

// ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("The OwnerId cannot be empty");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(c => CurrencyService.GetCurrency(c) != "")
            .WithMessage("The Currency cannot be empty and must be one of the list of currencies");

        RuleFor(x => x.InterestRate)
            .NotEmpty()
            .WithMessage("The InterestRate cannot be empty");

        RuleFor(x => x.CloseDate)
            .NotEmpty()
            .WithMessage("The CloseDate cannot be empty");
    }
}