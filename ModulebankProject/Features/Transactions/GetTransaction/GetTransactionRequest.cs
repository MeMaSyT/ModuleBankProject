using MediatR;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Transactions.GetTransaction;

/// <summary>
/// Request для получения транзакции
/// </summary>
/// <param name="Id">Номер транзакции</param>
public record GetTransactionRequest(Guid Id) : IRequest<MbResult<TransactionDto, ApiError>>;