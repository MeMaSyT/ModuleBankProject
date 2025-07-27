using CSharpFunctionalExtensions;
using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Features.Accounts
{
    public enum AccountType
    {
        Checking,
        Deposit,
        Credit
    }

    public class Account
    {
        private Account(
            Guid id,
            Guid ownerId,
            AccountType accountType,
            string currency,
            decimal balance,
            decimal interestRate,
            DateTime openDate,
            DateTime closeDate)
        {
            Id = id;
            OwnerId = ownerId;
            AccountType = accountType;
            Currency = currency;
            Balance = balance;
            InterestRate = interestRate;
            OpenDate = openDate;
            CloseDate = closeDate;
        }

        public Guid Id { get; }
        public Guid OwnerId { get; }
        public AccountType AccountType { get; }
        public string Currency { get; private set; }
        public decimal Balance { get; private set; }
        public decimal InterestRate { get; private set; }
        public DateTime OpenDate { get; }
        public DateTime CloseDate { get; private set; }
        public List<Transaction> Transactions { get; } = [];

        public static Result<Account> Create(
            Guid id,
            Guid ownerId,
            AccountType accountType,
            string currency,
            decimal interestRate,
            DateTime openDate,
            DateTime closeDate)
        {
            return Result.Success(
                new Account(id, ownerId, accountType, currency, 0M, interestRate, openDate, closeDate));
        }

        public void SetCurrency(string newCurrency)
        {
            Currency = newCurrency;
        }
        public void SetInterestRate(decimal newInterestRate)
        {
            InterestRate = newInterestRate;
        }
        public void SetCloseDatee(DateTime newCloseDate)
        {
            CloseDate = newCloseDate;
        }

        public bool ChangeBalance(decimal value)
        {
            decimal tempValue = Balance + value;
            if (tempValue < 0) return false;

            Balance = tempValue;
            return true;
        }
    }
}