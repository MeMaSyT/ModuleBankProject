namespace ModulebankProject.Infrastructure.RabbitMq.AntifraudConsumer;

public interface IAntifraudEventHandler
{
    Task HandleAsync(Guid clientId, bool isFreeze, CancellationToken cancellationToken);
}