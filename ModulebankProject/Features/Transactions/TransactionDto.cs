namespace ModulebankProject.Features.Transactions
{
    public record TransactionDto(
        Guid Id,
        Guid AccountId,
        Guid CounterpartyAccountId,
        decimal Amount,
        string Currency,
        TransactionType TransactionType,
        string Description,
        DateTime Time,
        TransactionStatus TransactionStatus
    );
}