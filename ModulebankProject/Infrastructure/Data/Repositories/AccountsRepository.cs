using AutoMapper;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Accounts.CreateAccount;
using ModulebankProject.Features.Accounts.EditAccount;
using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Infrastructure.Data.Repositories
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly IMyDataContext _myDataContext;
        private readonly IMapper _mapper;

        public AccountsRepository(IMyDataContext myDataContext, IMapper mapper)
        {
            _myDataContext = myDataContext;
            _mapper = mapper;
        }

        public async Task<Account> CreateAccount(CreateAccountCommand request)
        {
            Account creatingAccount = Account.Create(
                Guid.NewGuid(),
                request.OwnerId,
                request.AccountType,
                request.Сurrency,
                request.InterestRate,
                DateTime.UtcNow,
                request.CloseDate
            ).Value;

            _myDataContext.Accounts.Add(creatingAccount);
            await Task.Delay(500); //DataBase delay emulation

            return creatingAccount;
        }

        public async Task<Account?> EditAccount(EditAccountCommand request)
        {
            Account? editingAccount = _myDataContext.Accounts
                .FirstOrDefault(x => x.Id == request.Id);
            if (editingAccount != null)
            {
                editingAccount.SetCurrency(request.Сurrency ?? editingAccount.Currency);
                editingAccount.SetInterestRate(request.InterestRate ?? editingAccount.InterestRate);
                editingAccount.SetCloseDatee(request.CloseDate ?? editingAccount.CloseDate);
            }
            await Task.Delay(500); //DataBase delay emulation

            return editingAccount;
        }
        public async Task<Guid> DeleteAccount(Guid id)
        {
            Account? deletingAccount = _myDataContext.Accounts
                .FirstOrDefault(x => x.Id == id);
            if (deletingAccount == null) return Guid.Empty;

            _myDataContext.Accounts.Remove(deletingAccount);
            await Task.Delay(500); //DataBase delay emulation

            return id;
        }
        public async Task<List<Account>> GetAccounts(Guid ownerId)
        {
            List<Account> accounts = _myDataContext.Accounts
                .Where(a => a.OwnerId == ownerId).ToList();
            await Task.Delay(500); //DataBase delay emulation

            return accounts;
        }
        public async Task<AccountStatementDto?> GetAccountStatement(Guid id, DateTime startRangeDate, DateTime endRangeDate)
        {
            Account? account = _myDataContext.Accounts
                .FirstOrDefault(x => x.Id == id);
            if (account == null) return null;
            await Task.Delay(500); //DataBase delay emulation

            AccountStatementDto result = new AccountStatementDto(
                account.Id,
                account.OwnerId,
                account.AccountType,
                account.Currency,
                account.Balance,
                account.InterestRate,
                account.OpenDate,
                account.CloseDate,
                account.Transactions
                    .Where(t => t.Time >= startRangeDate && t.Time <= endRangeDate)
                    .Select(t => _mapper.Map<TransactionDto>(t))
                    .ToList());
            return result;
        }
        public async Task<Account?> GetAccountWithoutTransactions(Guid id)
        {
            Account? account = _myDataContext.Accounts
                .FirstOrDefault(x => x.Id == id);
            await Task.Delay(500); //DataBase delay emulation

            return account;
        }
        public async Task<bool> CheckAccountAvailability(Guid id)
        {
            Account? account = _myDataContext.Accounts
                .FirstOrDefault(x => x.Id == id);
            await Task.Delay(500); //DataBase delay emulation
            if (account == null) return false;

            return true;
        }
        public async Task<string> ApplyTransaction(Transaction transaction)
        {
            Account? account = _myDataContext.Accounts
                .FirstOrDefault(x => x.Id == transaction.AccountId);
            if (account == null) return "AccountNotFoundWhileApplyTransaction";

            bool staus = account.ChangeBalance(transaction.Amount * (int)transaction.TransactionType);
            if (!staus) return "ErrorWhileApplyTransaction";

            await Task.Delay(500); //DataBase delay emulation

            return "OK";
        }
    }
}