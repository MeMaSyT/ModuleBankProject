using MediatR;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.Infrastructure.Data.Repositories;

namespace ModulebankProject.Features.Transactions.TransferTransaction
{
    public class TransferTransactionCommandHandler : IRequestHandler<TransferTransactionCommand, TransferTransactionDto>
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IAccountsRepository _accountsRepository;

        public TransferTransactionCommandHandler(ITransactionsRepository transactionsRepository,
            IAccountsRepository accountsRepository)
        {
            _transactionsRepository = transactionsRepository;
            _accountsRepository = accountsRepository;
        }

        public async Task<TransferTransactionDto> Handle(TransferTransactionCommand request,
            CancellationToken cancellationToken)
        {
            Transaction? debit, credit;
            string debitResult = "", creditResult = "";

            //Transaction check
            Transaction? transaction = await _transactionsRepository.GetTransaction(request.Id);
            if (transaction == null)
                return new TransferTransactionDto(TransactionStatus.Error, "Transaction Not Found");
            if (transaction.TransactionStatus is
                TransactionStatus.Completed or
                TransactionStatus.InProcess or
                TransactionStatus.Error)
                return new TransferTransactionDto(transaction.TransactionStatus, "TransactionIsNotAvailable");
            if (!await _accountsRepository.CheckAccountAvailability(transaction.AccountId))
            {
                transaction.SetTransactionStatus(TransactionStatus.Error);
                return new TransferTransactionDto(transaction.TransactionStatus, "Account Not Found");
            }

            //СounterpartyTransaction check
            Transaction? counterpartyTransaction = null;
            if (transaction.CounterpartyAccountId != null)
            {
                counterpartyTransaction = await _transactionsRepository.RegisterTransaction(
                    new RegisterTransactionCommand(
                        (Guid)transaction.CounterpartyAccountId,
                        transaction.AccountId,
                        transaction.Amount,
                        transaction.Currency,
                        (TransactionType)(-(int)transaction.TransactionType),
                        transaction.Description));

                if (!await _accountsRepository.CheckAccountAvailability((Guid)transaction.CounterpartyAccountId))
                {
                    transaction.SetTransactionStatus(TransactionStatus.Error);
                    return new TransferTransactionDto(transaction.TransactionStatus, "CounterpartyAccount Not Found");
                }
            }

            //Start transaction process
            transaction.SetTransactionStatus(TransactionStatus.InProcess);
            bool availabilityOfMoney = true;
            if (transaction.TransactionType == TransactionType.Debit)
            {
                debit = transaction;
                credit = counterpartyTransaction;
                if (counterpartyTransaction != null)
                {
                    Account? counterpartyAccount =
                        await _accountsRepository.GetAccountWithoutTransactions(counterpartyTransaction.AccountId);
                    if (counterpartyAccount != null)
                    {
                        decimal tempBalance = counterpartyAccount.Balance;
                        if (tempBalance < transaction.Amount) availabilityOfMoney = false;
                    }
                }
            }
            else
            {
                debit = counterpartyTransaction;
                credit = transaction;
                Account? account = await _accountsRepository.GetAccountWithoutTransactions(transaction.AccountId);
                if (account != null)
                {
                    decimal tempBalance = account.Balance;
                    if (tempBalance < transaction.Amount) availabilityOfMoney = false;
                }
            }

            if (availabilityOfMoney)
            {
                if (credit != null) creditResult = await _accountsRepository.ApplyTransaction(credit);
                if (debit != null) debitResult = await _accountsRepository.ApplyTransaction(debit);
            }

            if (creditResult == "OK" && debitResult == "OK ")
            {
                transaction.SetTransactionStatus(TransactionStatus.Completed);
                return new TransferTransactionDto(transaction.TransactionStatus, "OK");
            }

            return new TransferTransactionDto(TransactionStatus.Error,
                $"creditOperationResult: {creditResult} \n debitOperationResult: {debitResult}");
        }
    }
}