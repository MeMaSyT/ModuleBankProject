using MediatR;

namespace ModulebankProject.Features.Accounts.GetAccounts
{
    public record GetAccountsRequest(Guid OwnerId) : IRequest<List<AccountDto>>;
}
