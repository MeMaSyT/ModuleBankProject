using ModulebankProject.Features.Transactions;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.MbResult;

namespace ModulebankProject.Infrastructure.Data.Repositories;

public interface ITransactionsRepository
{
    Task<MbResult<Transaction, ApiError>> RegisterTransaction(RegisterTransactionCommand request);
    Task<Transaction?> GetTransaction(Guid id);
    Task<MbResult<string, ApiError>> SetTransactionStatus(Guid id, TransactionStatus status);
}