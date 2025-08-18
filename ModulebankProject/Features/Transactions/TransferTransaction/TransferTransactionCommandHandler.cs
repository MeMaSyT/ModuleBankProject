using System.Text.Json;
using MediatR;
using ModulebankProject.Features.Inbox.Events;
using ModulebankProject.Features.Inbox.Events.AccountOpened;
using ModulebankProject.Features.Outbox;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.Infrastructure.Data.Repositories;
using ModulebankProject.MbResult;

namespace ModulebankProject.Features.Transactions.TransferTransaction
{
    // ReSharper disable once UnusedMember.Global используется медиатором, решарпер слишком глуп, чтобы это понять
    public class TransferTransactionCommandHandler : IRequestHandler<TransferTransactionCommand, MbResult<string, ApiError>>
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

        public async Task<MbResult<string, ApiError>> Handle(TransferTransactionCommand request,
            CancellationToken cancellationToken)
        {
            MbResult<string, ApiError> transactionResult;

            //Transaction check
            Transaction? transaction = await _transactionsRepository.GetTransaction(request.Id);
            if (transaction == null)
                return MbResult<string, ApiError>.Failure(new ApiError("Transaction Not Found", StatusCodes.Status404NotFound, "TransactionStatus: " + TransactionStatus.Error));
            if (transaction.TransactionStatus is
                TransactionStatus.Completed or
                TransactionStatus.InProcess)
                return MbResult<string, ApiError>.Failure(new ApiError("TransactionIsNotAvailable", StatusCodes.Status500InternalServerError, "TransactionStatus: " + transaction.TransactionStatus));
            if (!await _accountsRepository.CheckAccountAvailability(transaction.AccountId))
            {
                await _transactionsRepository.SetTransactionStatus(transaction.Id, TransactionStatus.Error);
                return MbResult<string, ApiError>.Failure(new ApiError("Account Not Found", StatusCodes.Status404NotFound, "TransactionStatus: " + TransactionStatus.Error));
            }

            //CounterpartyTransaction check
            Transaction? counterpartyTransaction = null;
            if (transaction.CounterpartyAccountId != null)
            {
                counterpartyTransaction = _transactionsRepository.RegisterTransaction(
                    new RegisterTransactionCommand(
                        (Guid)transaction.CounterpartyAccountId,
                        transaction.AccountId,
                        transaction.Amount,
                        transaction.Currency,
                        (TransactionType)(-(int)transaction.TransactionType),
                        transaction.Description)).Result.Result;

                if (!await _accountsRepository.CheckAccountAvailability((Guid)transaction.CounterpartyAccountId))
                {
                    await _transactionsRepository.SetTransactionStatus(transaction.Id, TransactionStatus.Error);
                    return MbResult<string, ApiError>.Failure(new ApiError("CounterpartyAccount Not Found", StatusCodes.Status404NotFound, "TransactionStatus: " + TransactionStatus.Error));
                }
            }

            //Start transaction process
            bool availabilityOfMoney = true;
            if (transaction.TransactionType == TransactionType.Debit)
            {
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
                var account = await _accountsRepository.GetAccountWithoutTransactions(transaction.AccountId);
                if (account != null)
                {
                    var tempBalance = account.Balance;
                    if (tempBalance < transaction.Amount) availabilityOfMoney = false;
                }
            }

            if (availabilityOfMoney)
            {
                transactionResult = await _accountsRepository.ApplyTransaction(transaction, counterpartyTransaction);
                if (!transactionResult.IsSuccess) return transactionResult;

                Transaction? debit = null;
                Transaction? credit = null;
                if (transaction.TransactionType == TransactionType.Debit)
                {
                    debit = transaction;
                    if (counterpartyTransaction != null) credit = counterpartyTransaction;
                }
                else
                {
                    credit = transaction;
                    if (counterpartyTransaction != null) debit = counterpartyTransaction;
                }

                if (debit != null)
                {
                    request.Events.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Content = JsonSerializer.Serialize(new MoneyDebitedEvent
                        {
                            AccountId = debit.AccountId,
                            Amount = debit.Amount,
                            Currency = debit.Currency,
                            OperationId = debit.Id
                        }),
                        Error = "",
                        Type = "money.*",
                        Properties = new Dictionary<string, object>
                        {
                            ["Type"] = "Debit"
                        }
                    });
                }
                if (credit != null)
                {
                    request.Events.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Content = JsonSerializer.Serialize(new MoneyCreditedEvent()
                        {
                            AccountId = credit.AccountId,
                            Amount = credit.Amount,
                            Currency = credit.Currency,
                            OperationId = credit.Id
                        }),
                        Error = "",
                        Type = "account.*",
                        Properties = new Dictionary<string, object>
                        {
                            ["Type"] = "Credit"
                        }
                    });
                }
                request.Events.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Content = JsonSerializer.Serialize(new TransferCompletedEvent()
                    {
                        SourceAccountId = transaction.AccountId,
                        DestinationAccountId = counterpartyTransaction?.AccountId ?? Guid.Empty,
                        Amount = transaction.Amount,
                        Currency = transaction.Currency,
                        TransferId = transaction.Id
                    }),
                    Error = "",
                    Type = "account.*",
                    Properties = new Dictionary<string, object>
                    {
                        ["Type"] = "Transfer"
                    }
                });
            }
            else return MbResult<string, ApiError>.Failure(new ApiError("Insufficient Funds", StatusCodes.Status403Forbidden));

            if (transactionResult.Result == "OK")
            {
                await _transactionsRepository.SetTransactionStatus(transaction.Id, TransactionStatus.Completed);
                return MbResult<string, ApiError>.Success(TransactionStatus.Completed.ToString());
            }

            return MbResult<string, ApiError>.Failure(
                new ApiError("Transaction Error",
                    StatusCodes.Status500InternalServerError, 
                    "TransactionStatus: " + TransactionStatus.Error +  $" OperationResult: {transactionResult}"));
        }
    }
}