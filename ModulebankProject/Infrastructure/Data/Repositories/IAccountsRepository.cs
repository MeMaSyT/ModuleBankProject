using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Accounts.CreateAccount;
using ModulebankProject.Features.Accounts.EditAccount;
using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Infrastructure.Data.Repositories;

public interface IAccountsRepository
{
    Task<Account> CreateAccount(CreateAccountCommand request);
    Task<Account?> EditAccount(EditAccountCommand request);
    Task<Guid> DeleteAccount(Guid id);
    Task<List<Account>> GetAccounts(Guid ownerId);
    Task<AccountStatementDto?> GetAccountStatement(Guid id, DateTime startRangeDate, DateTime endRangeDate);
    Task<bool> CheckAccountAvailability(Guid id);
    Task<string> ApplyTransaction(Transaction transaction);
    Task<Account?> GetAccountWithoutTransactions(Guid id);
}