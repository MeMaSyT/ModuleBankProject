using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.EditAccount;

/// <summary>
/// Command для редактирования счёта
/// </summary>
/// <param name="Id">Номер счёта</param>
/// <param name="Currency">Валюта</param>
/// <param name="InterestRate">Процентная ставка</param>
/// <param name="CloseDate">Дата закрытия счёта</param>
public record EditAccountCommand
(
    Guid? Id,
    string? Currency,
    decimal? InterestRate,
    DateTime? CloseDate
) : IRequest<MbResult<AccountDto, ApiError>>;