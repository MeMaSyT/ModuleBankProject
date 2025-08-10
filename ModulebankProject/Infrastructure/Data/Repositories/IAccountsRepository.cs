using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Accounts.CreateAccount;
using ModulebankProject.Features.Accounts.EditAccount;
using ModulebankProject.Features.Transactions;
using ModulebankProject.MbResult;

namespace ModulebankProject.Infrastructure.Data.Repositories;

public interface IAccountsRepository
{
    Task<MbResult<Account, ApiError>> CreateAccount(CreateAccountCommand request);
    Task<MbResult<Account, ApiError>> EditAccount(EditAccountCommand request);
    Task<MbResult<Guid, ApiError>> DeleteAccount(Guid id);
    Task<List<Account>> GetAccounts(Guid ownerId);
    Task<AccountStatementDto?> GetAccountStatement(Guid id, DateTime startRangeDate, DateTime endRangeDate);
    Task<bool> CheckAccountAvailability(Guid id);
    Task<MbResult<string, ApiError>> ApplyTransaction(Transaction transaction, Transaction? counterpartyTransaction = null);
    Task<Account?> GetAccountWithoutTransactions(Guid id);
}