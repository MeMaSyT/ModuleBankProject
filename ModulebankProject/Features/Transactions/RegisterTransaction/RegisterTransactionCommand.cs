using MediatR;

namespace ModulebankProject.Features.Transactions.RegisterTransaction
{
    public record RegisterTransactionCommand(
        Guid AccountId,
        Guid? CounterpartyAccountId,
        decimal Amount,
        string Currency,
        TransactionType TransactionType,
        string Description) : IRequest<TransactionDto>;
}