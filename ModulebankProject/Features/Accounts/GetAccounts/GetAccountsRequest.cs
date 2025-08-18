using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.GetAccounts;

/// <summary>
/// Request для получения счетов клиента
/// </summary>
/// <param name="OwnerId">Номер клиента</param>
public record GetAccountsRequest(Guid OwnerId) : IRequest<MbResult<List<AccountDto>, ApiError>>;