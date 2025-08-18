using FluentValidation;
using ModulebankProject.Infrastructure;

namespace ModulebankProject.Features.Transactions.RegisterTransaction;

// ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
public class RegisterTransactionCommandValidator : AbstractValidator<RegisterTransactionCommand>
{
    public RegisterTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .NotEqual(Guid.Empty)
            .WithMessage("The AccountId cannot be empty");
            

        RuleFor(x => x.Amount)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("The Amount cannot be empty or less than 1");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(c => CurrencyService.GetCurrency(c) != "")
            .WithMessage("The Currency cannot be empty and must be one of the list of currencies");
    }
}