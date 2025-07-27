using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Accounts.GetAccounts
{
    public class GetAccountsRequestHandler : IRequestHandler<GetAccountsRequest, List<AccountDto>>
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;

        public GetAccountsRequestHandler(IMapper mapper, IAccountsRepository accountsRepository)
        {
            _mapper = mapper;
            _accountsRepository = accountsRepository;
        }
        public async Task<List<AccountDto>> Handle(GetAccountsRequest request, CancellationToken cancellationToken)
        {
            var accounts = await _accountsRepository
                .GetAccounts(request.OwnerId);
            return accounts
                .Select(a => _mapper.Map<AccountDto>(a))
                .ToList();
        }
    }
}
