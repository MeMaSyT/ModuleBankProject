using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Features.Transactions.RegisterTransaction;

namespace ModulebankProject.Infrastructure.Data.Repositories
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly IMyDataContext _myDataContext;

        public TransactionsRepository(IMyDataContext myDataContext)
        {
            _myDataContext = myDataContext;
        }

        public async Task<Transaction> RegisterTransaction(RegisterTransactionCommand request)
        {
            Transaction registeringTransaction = Transaction
                .Create(
                    Guid.NewGuid(),
                    request.AccountId,
                    request.CounterpartyAccountId,
                    request.Amount,
                    request.Currency,
                    request.TransactionType,
                    request.Description,
                    DateTime.UtcNow,
                    TransactionStatus.Registered).Value;
            _myDataContext.Transactions.Add(registeringTransaction);

            Account? account = _myDataContext.Accounts.FirstOrDefault(a => a.Id == request.AccountId);
            if (account == null) throw new Exception("Account of Transaction not found");
            account.Transactions.Add(registeringTransaction);
            await Task.Delay(500); //DataBase delay emulation

            return registeringTransaction;
        }
        public async Task<Transaction?> GetTransaction(Guid id)
        {
            Transaction? transaction = _myDataContext.Transactions
                .FirstOrDefault(t => t.Id == id);
            await Task.Delay(500); //DataBase delay emulation

            return transaction;
        }
    }
}
