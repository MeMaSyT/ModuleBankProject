using MediatR;

namespace ModulebankProject.Features.Accounts.CreateAccount
{
    public record CreateAccountCommand (
        Guid OwnerId,
        AccountType AccountType,
        string Сurrency,
        decimal InterestRate,
        DateTime CloseDate
    ) : IRequest<AccountDto>;
}