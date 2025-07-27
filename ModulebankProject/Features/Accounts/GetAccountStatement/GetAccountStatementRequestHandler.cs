using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Accounts.GetAccountStatement
{
    public class GetAccountStatementRequestHandler : IRequestHandler<GetAccountStatementRequest, AccountStatementDto?>
    {
        private readonly IAccountsRepository _accountsRepository;

        public GetAccountStatementRequestHandler(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }
        public async Task<AccountStatementDto?> Handle(GetAccountStatementRequest request, CancellationToken cancellationToken)
        {
            return await _accountsRepository.GetAccountStatement(request.Id, request.StartRangeDate, request.EndRangeDate);
        }
    }
}
