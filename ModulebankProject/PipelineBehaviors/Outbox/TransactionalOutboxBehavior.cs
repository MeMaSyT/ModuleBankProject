using MediatR;
using ModulebankProject.Infrastructure.Data;

namespace ModulebankProject.PipelineBehaviors.Outbox;

public class TransactionalOutboxBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ModulebankDataContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
    public TransactionalOutboxBehavior(
        ModulebankDataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICommandWithEvents commandWithEvents)
        {
            return await next(cancellationToken);
        }
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(cancellationToken);

            if (commandWithEvents.Events.Count > 0)
            {
                foreach (var outboxMessage in commandWithEvents.Events)
                {
                    outboxMessage.OccurredOn = DateTime.UtcNow;
                    await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            //await _eventPublisher.PublishPendingEventsAsync(cancellationToken);

            return response;
        }
        catch(Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            Console.WriteLine("ERROR: " + e);
            throw;
        }
    }
}