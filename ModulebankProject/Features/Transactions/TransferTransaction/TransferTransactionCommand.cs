using MediatR;
using ModulebankProject.Features.Outbox;
using ModulebankProject.MbResult;
using ModulebankProject.PipelineBehaviors.Outbox;

namespace ModulebankProject.Features.Transactions.TransferTransaction;

/// <summary>
/// Command для проведения транзакции
/// </summary>
/// <param name="Id">Номер транзакции</param>
public record TransferTransactionCommand(Guid Id) : IRequest<MbResult<string, ApiError>>, ICommandWithEvents
{
    public List<OutboxMessage> Events { get; } = [];
}