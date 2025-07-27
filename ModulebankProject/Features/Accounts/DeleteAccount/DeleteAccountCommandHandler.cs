using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Accounts.DeleteAccount
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Guid>
    {
        private readonly IAccountsRepository _accountsRepository;

        public DeleteAccountCommandHandler(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }
        public async Task<Guid> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            return await _accountsRepository.DeleteAccount(request.Id);
        }
    }
}
