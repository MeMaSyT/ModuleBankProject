using MediatR;

namespace ModulebankProject.Features.Transactions.TransferTransaction
{
    public record TransferTransactionCommand(Guid Id) : IRequest<TransferTransactionDto>;
}
