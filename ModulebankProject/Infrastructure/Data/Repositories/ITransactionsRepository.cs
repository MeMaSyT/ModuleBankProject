using ModulebankProject.Features.Transactions;
using ModulebankProject.Features.Transactions.RegisterTransaction;

namespace ModulebankProject.Infrastructure.Data.Repositories;

public interface ITransactionsRepository
{
    Task<Transaction?> RegisterTransaction(RegisterTransactionCommand request);
    Task<Transaction?> GetTransaction(Guid id);
}