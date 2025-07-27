namespace ModulebankProject.Features.Transactions;

public interface ITransactionsService
{
    Task<Transaction> RegisterTransaction(RegisterTransactionDto dto);
    Task<(TransactionStatus Status, string Description)> TransferTransaction(Guid id);
    Task<Transaction?> GetTransaction(Guid id);
}