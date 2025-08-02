using MediatR;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Transactions.TransferTransaction
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class TransferTransactionCommandHandler : IRequestHandler<TransferTransactionCommand, MbResult<TransactionStatus, ApiError>>
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IAccountsRepository _accountsRepository;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public TransferTransactionCommandHandler(ITransactionsRepository transactionsRepository,
            IAccountsRepository accountsRepository)
        {
            _transactionsRepository = transactionsRepository;
            _accountsRepository = accountsRepository;
        }

        public async Task<MbResult<TransactionStatus, ApiError>> Handle(TransferTransactionCommand request,
            CancellationToken cancellationToken)
        {
            Transaction? debit, credit;
            string debitResult = "", creditResult = "";

            //Transaction check
            Transaction? transaction = await _transactionsRepository.GetTransaction(request.Id);
            if (transaction == null)
                return MbResult<TransactionStatus, ApiError>.Failure(new ApiError("Transaction Not Found", StatusCodes.Status404NotFound, "TransactionStatus: " + TransactionStatus.Error));
            if (transaction.TransactionStatus is
                TransactionStatus.Completed or
                TransactionStatus.InProcess or
                TransactionStatus.Error)
                return MbResult<TransactionStatus, ApiError>.Failure(new ApiError("TransactionIsNotAvailable", StatusCodes.Status404NotFound, "TransactionStatus: " + TransactionStatus.Error));
            if (!await _accountsRepository.CheckAccountAvailability(transaction.AccountId))
            {
                transaction.TransactionStatus = TransactionStatus.Error;
                return MbResult<TransactionStatus, ApiError>.Failure(new ApiError("Account Not Found", StatusCodes.Status404NotFound, "TransactionStatus: " + TransactionStatus.Error));
            }

            //CounterpartyTransaction check
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
                    transaction.TransactionStatus = TransactionStatus.Error;
                    return MbResult<TransactionStatus, ApiError>.Failure(new ApiError("CounterpartyAccount Not Found", StatusCodes.Status404NotFound, "TransactionStatus: " + TransactionStatus.Error));
                }
            }

            //Start transaction process
            transaction.TransactionStatus = TransactionStatus.InProcess;
            bool availabilityOfMoney = true;
            if (transaction.TransactionType == TransactionType.Debit)
            {
                debit = transaction;
                credit = counterpartyTransaction;
                if (counterpartyTransaction != null)
                {
                    var counterpartyAccount =
                        await _accountsRepository.GetAccountWithoutTransactions(counterpartyTransaction.AccountId);
                    if (counterpartyAccount != null)
                    {
                        var tempBalance = counterpartyAccount.Balance;
                        if (tempBalance < transaction.Amount) availabilityOfMoney = false;
                    }
                }
            }
            else
            {
                debit = counterpartyTransaction;
                credit = transaction;
                var account = await _accountsRepository.GetAccountWithoutTransactions(transaction.AccountId);
                if (account != null)
                {
                    var tempBalance = account.Balance;
                    if (tempBalance < transaction.Amount) availabilityOfMoney = false;
                }
            }

            if (availabilityOfMoney)
            {
                if (credit != null) creditResult = await _accountsRepository.ApplyTransaction(credit);
                if (debit != null) debitResult = await _accountsRepository.ApplyTransaction(debit);
            }
            else return MbResult<TransactionStatus, ApiError>.Failure(new ApiError("Insufficient Funds", StatusCodes.Status403Forbidden));

            if (creditResult == "OK" && debitResult == "OK ")
            {
                transaction.TransactionStatus = TransactionStatus.Completed;
                return MbResult<TransactionStatus, ApiError>.Success(transaction.TransactionStatus);
            }

            return MbResult<TransactionStatus, ApiError>.Failure(
                new ApiError("Transaction Error",
                    StatusCodes.Status500InternalServerError, 
                    "TransactionStatus: " + TransactionStatus.Error +  $"  creditOperationResult: {creditResult}   debitOperationResult: {debitResult}"));
        }
    }
}