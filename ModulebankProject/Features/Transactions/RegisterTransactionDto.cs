namespace ModulebankProject.Features.Transactions
{
    public record RegisterTransactionDto
    (
        Guid AccountId,
        Guid CounterpartyAccountId,
        decimal Amount,
        string Сurrency,
        TransactionType TransactionType,
        string Description
    );
}
