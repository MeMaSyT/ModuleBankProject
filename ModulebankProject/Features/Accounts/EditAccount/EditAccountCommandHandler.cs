using AutoMapper;
using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.EditAccount
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class EditAccountCommandHandler : IRequestHandler<EditAccountCommand, MbResult<AccountDto, ApiError>>
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public EditAccountCommandHandler(IMapper mapper, IAccountsRepository accountsRepository)
        {
            _mapper = mapper;
            _accountsRepository = accountsRepository;
        }

        public async Task<MbResult<AccountDto, ApiError>> Handle(EditAccountCommand request,
            CancellationToken cancellationToken)
        {
            var editingAccount = await _accountsRepository.EditAccount(request);
            if (!editingAccount.IsSuccess) return MbResult<AccountDto, ApiError>.Failure(editingAccount.Error!);

            return MbResult<AccountDto, ApiError>.Success(_mapper.Map<AccountDto>(editingAccount.Result));
        }
    }
}