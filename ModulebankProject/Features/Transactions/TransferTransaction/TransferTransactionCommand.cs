using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Transactions.TransferTransaction
{
    /// <summary>
    /// Command для проведения транзакции
    /// </summary>
    /// <param name="Id">Номер транзакции</param>
    public record TransferTransactionCommand(Guid Id) : IRequest<MbResult<string, ApiError>>;
}
