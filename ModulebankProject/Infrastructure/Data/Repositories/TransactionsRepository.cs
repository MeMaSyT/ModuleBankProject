using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.MbResult;

namespace ModulebankProject.Infrastructure.Data.Repositories;

public class TransactionsRepository : ITransactionsRepository
{
    private readonly ModulebankDataContext _dataContext;

    // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
    public TransactionsRepository(ModulebankDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<MbResult<Transaction, ApiError>> RegisterTransaction(RegisterTransactionCommand request)
    {
        var account = await _dataContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);
        if (account == null) return MbResult<Transaction, ApiError>.Failure(new ApiError("AccountNotFound", StatusCodes.Status404NotFound));

        var registeringTransaction = new Transaction(
            Guid.NewGuid(),
            request.AccountId,
            request.CounterpartyAccountId,
            request.Amount,
            request.Currency,
            request.TransactionType,
            request.Description,
            DateTime.UtcNow,
            TransactionStatus.Registered);
        await _dataContext.Transactions.AddAsync(registeringTransaction);
        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return MbResult<Transaction, ApiError>.Failure(new ApiError("Conflict", StatusCodes.Status409Conflict));
        }

        return MbResult<Transaction, ApiError>.Success(registeringTransaction);
    }
    public async Task<Transaction?> GetTransaction(Guid id)
    {
        var transaction = await _dataContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return transaction;
    }
    public async Task<MbResult<string, ApiError>> SetTransactionStatus(Guid id, TransactionStatus status)
    {
        var transaction = await _dataContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null) return MbResult<string, ApiError>.Failure(new ApiError("TransactionNotFound", StatusCodes.Status404NotFound));

        transaction.TransactionStatus = status;
        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return MbResult<string, ApiError>.Failure(new ApiError("Conflict", StatusCodes.Status409Conflict));
        }

        return MbResult<string, ApiError>.Success("Done");
    }
}