using CSharpFunctionalExtensions;

namespace ModulebankProject.Features.Transactions
{
    public enum TransactionType
    {
        Debit = 1,
        Credit = -1
    }

    public enum TransactionStatus
    {
        Registered,
        InProcess,
        Completed,
        Error
    }

    public class Transaction
    {
        private Transaction(
            Guid id,
            Guid accountId,
            Guid? counterpartyAccountId,
            decimal amount,
            string currency,
            TransactionType transactionType,
            string description,
            DateTime time,
            TransactionStatus transactionStatus)
        {
            Id = id;
            AccountId = accountId;
            CounterpartyAccountId = counterpartyAccountId;
            Amount = amount;
            Currency = currency;
            TransactionType = transactionType;
            Description = description;
            Time = time;
            TransactionStatus = transactionStatus;
        }

        public Guid Id { get; }
        public Guid AccountId { get; }
        public Guid? CounterpartyAccountId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public TransactionType TransactionType { get; }
        public string Description { get; }
        public DateTime Time { get; }
        public TransactionStatus TransactionStatus { get; private set; }

        public static Result<Transaction> Create(
            Guid id,
            Guid accountId,
            Guid? counterpartyAccountId,
            decimal amount,
            string currency,
            TransactionType transactionType,
            string description,
            DateTime time,
            TransactionStatus transactionStatus)
        {
            return Result.Success(
                new Transaction(
                    id, 
                    accountId, 
                    counterpartyAccountId, 
                    amount, 
                    currency, 
                    transactionType, 
                    description,
                    time, 
                    transactionStatus));
        }

        public void SetTransactionStatus(TransactionStatus newStatus)
        {
            TransactionStatus = newStatus;
        }
    }
}