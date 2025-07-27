using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Accounts.EditAccount
{
    public class EditAccountCommandHandler : IRequestHandler<EditAccountCommand, AccountDto?>
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;

        public EditAccountCommandHandler(IMapper mapper, IAccountsRepository accountsRepository)
        {
            _mapper = mapper;
            _accountsRepository = accountsRepository;
        }
        public async Task<AccountDto?> Handle(EditAccountCommand request, CancellationToken cancellationToken)
        {
            Account? editingAccount = await _accountsRepository.EditAccount(request);
            if (editingAccount != null)
            {
                return _mapper.Map<AccountDto>(editingAccount);
            }
            return null;
        }
    }
}
