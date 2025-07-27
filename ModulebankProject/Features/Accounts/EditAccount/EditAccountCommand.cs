using MediatR;

namespace ModulebankProject.Features.Accounts.EditAccount
{
    public record EditAccountCommand
    (
        Guid? Id,
        string? Сurrency,
        decimal? InterestRate,
        DateTime? CloseDate
    ) : IRequest<AccountDto?>;
}