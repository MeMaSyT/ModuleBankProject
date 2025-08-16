using AutoMapper;
using MediatR;
using ModulebankProject.Features.Inbox.Events.AccountOpened;
using ModulebankProject.Features.Outbox;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;
using System;
using System.Text.Json;

namespace ModulebankProject.Features.Accounts.CreateAccount
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, MbResult<AccountDto, ApiError>>
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public CreateAccountCommandHandler(IAccountsRepository accountsRepository, IMapper mapper)
        {
            _accountsRepository = accountsRepository;
            _mapper = mapper;
        }
        public async Task<MbResult<AccountDto, ApiError>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var createdAccount = await _accountsRepository.CreateAccount(request);
            if (!createdAccount.IsSuccess) return MbResult<AccountDto, ApiError>.Failure(createdAccount.Error!);
            request.Events.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Content = JsonSerializer.Serialize(new AccountOpenedEvent
                {
                    AccountId = createdAccount.Result!.Id,
                    OwnerId = createdAccount.Result.OwnerId,
                    Currency = createdAccount.Result.Currency,
                    Type = createdAccount.Result.AccountType
                }),
                Error = "",
                Type = "account.opened"
            });
            var createDto = _mapper.Map<AccountDto>(createdAccount.Result);
            if(createDto == null) return MbResult<AccountDto, ApiError>.Failure(new ApiError("Mapping Error", StatusCodes.Status500InternalServerError));
            return MbResult<AccountDto, ApiError>.Success(createDto);
        }
    }
}
