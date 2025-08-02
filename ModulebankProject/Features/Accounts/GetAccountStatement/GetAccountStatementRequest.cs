using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Accounts.GetAccountStatement
{
    /// <summary>
    /// Request для получения выписки по счёту
    /// </summary>
    /// <param name="Id">Номер счёта</param>
    /// <param name="StartRangeDate">С какого числа</param>
    /// <param name="EndRangeDate">По какое число</param>
    public record GetAccountStatementRequest(Guid Id, DateTime StartRangeDate, DateTime EndRangeDate) : IRequest<MbResult<AccountStatementDto, ApiError>>;
}
