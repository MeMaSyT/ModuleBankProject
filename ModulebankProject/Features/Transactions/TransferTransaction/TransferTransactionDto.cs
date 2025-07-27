namespace ModulebankProject.Features.Transactions.TransferTransaction
{
    public record TransferTransactionDto(
        TransactionStatus Status,
        string Description
    );
}