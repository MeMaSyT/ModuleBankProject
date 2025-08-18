using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Accounts.CreateAccount;
using ModulebankProject.Features.Accounts.EditAccount;
using ModulebankProject.Features.Transactions;
using ModulebankProject.MbResult;
using Npgsql;

namespace ModulebankProject.Infrastructure.Data.Repositories
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly ModulebankDataContext _dataContext;
        private readonly IMapper _mapper;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public AccountsRepository(ModulebankDataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<MbResult<Account, ApiError>> CreateAccount(CreateAccountCommand request)
        {
            var creatingAccount = new Account(
                Guid.NewGuid(),
                request.OwnerId,
                request.AccountType,
                request.Currency,
                0M,
                request.InterestRate,
                DateTime.UtcNow,
                request.CloseDate
            );

            await _dataContext.Accounts.AddAsync(creatingAccount);
            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return MbResult<Account, ApiError>.Failure(new ApiError("Conflict", StatusCodes.Status409Conflict));
            }
            

            return MbResult<Account, ApiError>.Success(creatingAccount);
        }

        public async Task<MbResult<Account, ApiError>> EditAccount(EditAccountCommand request)
        {
            var editingAccount = await _dataContext.Accounts
                .FirstOrDefaultAsync(x => x.Id == request.Id);
            if (editingAccount == null)
                return MbResult<Account, ApiError>.Failure(new ApiError("AccountNotFound",
                    StatusCodes.Status404NotFound));

            editingAccount.Currency = request.Currency ?? editingAccount.Currency;
            editingAccount.InterestRate = request.InterestRate ?? editingAccount.InterestRate;
            editingAccount.CloseDate = request.CloseDate ?? editingAccount.CloseDate;

            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return MbResult<Account, ApiError>.Failure(new ApiError("Conflict", StatusCodes.Status409Conflict));
            }

            return MbResult<Account, ApiError>.Success(editingAccount);
        }
        public async Task<MbResult<Guid, ApiError>> DeleteAccount(Guid id)
        {
            int deletedAmount;
            try
            {
                deletedAmount = await _dataContext.Accounts.Where(a => a.Id == id).ExecuteDeleteAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return MbResult<Guid, ApiError>.Failure(new ApiError("Conflict", StatusCodes.Status409Conflict));
            }
            
            if (deletedAmount == 0) return MbResult<Guid, ApiError>.Failure(new ApiError("AccountNotFound",
                StatusCodes.Status404NotFound));

            return MbResult<Guid, ApiError>.Success(id);
        }
        public async Task<List<Account>> GetAccounts(Guid ownerId)
        {
            List<Account> accounts = await _dataContext.Accounts
                .AsNoTracking()
                .Where(a => a.OwnerId == ownerId)
                .ToListAsync();

            return accounts;
        }
        public async Task<AccountStatementDto?> GetAccountStatement(Guid id, DateTime startRangeDate, DateTime endRangeDate)
        {
            var account = await _dataContext.Accounts
                .AsNoTracking()
                .Include(account => account.Transactions)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (account == null) return null;

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
            var account = await _dataContext.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return account;
        }
        public async Task<bool> CheckAccountAvailability(Guid id)
        {
            var account = await _dataContext.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (account == null) return false;

            return true;
        }
        public async Task<MbResult<string, ApiError>> ApplyTransaction(Transaction transaction, Transaction? counterpartyTransaction = null)
        {
            // ReSharper disable once ConvertToUsingDeclaration решарпер портит читабельность
            await using (var t =
                         await _dataContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var tr =
                        await _dataContext.Transactions.FirstOrDefaultAsync(tr => tr.Id == transaction.Id);
                    tr!.TransactionStatus = TransactionStatus.InProcess;

                    var counterpartyResult = 0M;
                    Account? counterpartyAccount = null;

                    //Get accounts
                    var account = await _dataContext.Accounts
                        .FirstOrDefaultAsync(x => x.Id == transaction.AccountId);
                    if (account == null)
                    {
                        await t.RollbackAsync();
                        return MbResult<string, ApiError>.Failure(new ApiError("AccountNotFoundWhileApplyTransaction", StatusCodes.Status404NotFound));
                    }
                    if (account.Freezing == true)
                    {
                        await t.RollbackAsync();
                        return MbResult<string, ApiError>.Failure(new ApiError("AccountIsFreezing", StatusCodes.Status409Conflict));
                    }

                    if (counterpartyTransaction != null)
                    {
                        counterpartyAccount = await _dataContext.Accounts
                            .FirstOrDefaultAsync(x => x.Id == counterpartyTransaction.AccountId);
                        if (counterpartyAccount == null)
                        {
                            await t.RollbackAsync();
                            return MbResult<string, ApiError>.Failure(
                                new ApiError("CounterpartyAccountNotFoundWhileApplyTransaction",
                                    StatusCodes.Status404NotFound));
                        }
                        if (counterpartyAccount.Freezing == true)
                        {
                            await t.RollbackAsync();
                            return MbResult<string, ApiError>.Failure(new ApiError("CounterpartyAccountIsFreezing", StatusCodes.Status409Conflict));
                        }
                    }

                    //Set before balances
                    var beforeAccountBalance = account.Balance;
                    var beforeCounterpartyAccountBalance = 0M;
                    if (counterpartyAccount != null) beforeCounterpartyAccountBalance = counterpartyAccount.Balance;

                    //Get Statuses
                    var status = AccountHelper.TryChangeBalance(beforeAccountBalance,
                        transaction.Amount * (int)transaction.TransactionType, out var result);
                    if (!status)
                    {
                        await t.RollbackAsync();
                        return MbResult<string, ApiError>.Failure(new ApiError("ErrorWhileApplyTransaction", StatusCodes.Status403Forbidden));
                    }

                    if (counterpartyTransaction != null)
                    {
                        var counterpartyStatus = AccountHelper.TryChangeBalance(counterpartyAccount!.Balance,
                            counterpartyTransaction.Amount * (int)counterpartyTransaction.TransactionType, out counterpartyResult);
                        if (!counterpartyStatus)
                        {
                            await t.RollbackAsync();
                            return MbResult<string, ApiError>.Failure(
                                new ApiError("ErrorWhileApplyCounterpartyTransaction", StatusCodes.Status404NotFound));
                        }
                    }

                    //Change balances
                    account.Balance = result;
                    if (counterpartyAccount != null)
                    {
                        beforeCounterpartyAccountBalance = counterpartyAccount.Balance;
                        counterpartyAccount.Balance = counterpartyResult;
                    }

                    //Сверка сумм
                    if (beforeAccountBalance != result - transaction.Amount * (int)transaction.TransactionType)
                    {
                        await t.RollbackAsync();
                        return MbResult<string, ApiError>.Failure(
                            new ApiError($"TransactionValuesNotEqual: before={beforeAccountBalance} checkValue={result - transaction.Amount * (int)transaction.TransactionType} ", StatusCodes.Status500InternalServerError));
                    }
                    if (counterpartyTransaction != null)
                    {
                        if (beforeCounterpartyAccountBalance != counterpartyResult - counterpartyTransaction.Amount * (int)counterpartyTransaction.TransactionType)
                        {
                            await t.RollbackAsync();
                            return MbResult<string, ApiError>.Failure(
                                new ApiError($"TransactionValuesNotEqual: before={beforeCounterpartyAccountBalance} checkValue={counterpartyResult - counterpartyTransaction.Amount * -(int)transaction.TransactionType} ", StatusCodes.Status500InternalServerError));
                        }
                    }

                    try
                    {
                        await _dataContext.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        await t.RollbackAsync();
                        return MbResult<string, ApiError>.Failure(new ApiError("Concurrency conflict", StatusCodes.Status409Conflict));
                    }
                    catch (InvalidOperationException ex) when (ex.InnerException is DbUpdateException
                                                               {
                                                                   InnerException: PostgresException pgEx
                                                                   // ReSharper disable once MergeIntoPattern предлагает фигню
                                                               } &&
                                                               pgEx.SqlState == "40001")
                    {
                        return MbResult<string, ApiError>.Failure(new ApiError("Concurrency conflict", StatusCodes.Status409Conflict));
                    }
                    await t.CommitAsync();

                    return MbResult<string, ApiError>.Success("OK");
                }
                catch (PostgresException e)
                {
                    await t.RollbackAsync();
                    Console.WriteLine("Error: " + e);
                    return MbResult<string, ApiError>.Failure(new ApiError(e.Message, StatusCodes.Status500InternalServerError));
                }
            }
        }
    }
}