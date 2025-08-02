using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.GetAccounts
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class GetAccountsRequestHandler : IRequestHandler<GetAccountsRequest, MbResult<List<AccountDto>, ApiError>>
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public GetAccountsRequestHandler(IMapper mapper, IAccountsRepository accountsRepository)
        {
            _mapper = mapper;
            _accountsRepository = accountsRepository;
        }
        public async Task<MbResult<List<AccountDto>, ApiError>> Handle(GetAccountsRequest request, CancellationToken cancellationToken)
        {
            var accounts = await _accountsRepository
                .GetAccounts(request.OwnerId);
            return MbResult<List<AccountDto>, ApiError>.Success(accounts
                .Select(a => _mapper.Map<AccountDto>(a))
                .ToList());
        }
    }
}
