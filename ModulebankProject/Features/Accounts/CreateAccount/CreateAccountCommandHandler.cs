using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Accounts.CreateAccount
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;

        public CreateAccountCommandHandler(IAccountsRepository accountsRepository, IMapper mapper)
        {
            _accountsRepository = accountsRepository;
            _mapper = mapper;
        }
        public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            return _mapper.Map<AccountDto>(await _accountsRepository.CreateAccount(request));
        }
    }
}
