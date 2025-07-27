using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Accounts.CheckAccountAvailability
{
    public class CheckAccountAvailabilityRequestHandler : IRequestHandler<CheckAccountAvailabilityRequest, bool>
    {
        private readonly IAccountsRepository _accountsRepository;

        public CheckAccountAvailabilityRequestHandler(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }
        public async Task<bool> Handle(CheckAccountAvailabilityRequest request, CancellationToken cancellationToken)
        {
            return await _accountsRepository.CheckAccountAvailability(request.Id);
        }
    }
}
