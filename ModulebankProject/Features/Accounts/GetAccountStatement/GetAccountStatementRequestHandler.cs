using MediatR;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.GetAccountStatement;

// ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
public class GetAccountStatementRequestHandler : IRequestHandler<GetAccountStatementRequest, MbResult<AccountStatementDto, ApiError>>
{
    private readonly IAccountsRepository _accountsRepository;

    // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
    public GetAccountStatementRequestHandler(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }
    public async Task<MbResult<AccountStatementDto, ApiError>> Handle(GetAccountStatementRequest request, CancellationToken cancellationToken)
    {
        var statement =
            await _accountsRepository.GetAccountStatement(request.Id, request.StartRangeDate, request.EndRangeDate);
        return statement == null ? MbResult<AccountStatementDto, ApiError>.Failure(new ApiError("Account Not Found", StatusCodes.Status404NotFound)) : MbResult<AccountStatementDto, ApiError>.Success(statement);
    }
}