using MediatR;

namespace ModulebankProject.Features.Accounts.DeleteAccount
{
    public record DeleteAccountCommand(Guid Id) : IRequest<Guid>;
}
