using MediatR;

namespace ModulebankProject.Features.Transactions.GetTransaction
{
    public record GetTransactionRequest(Guid Id) : IRequest<TransactionDto?>;
}