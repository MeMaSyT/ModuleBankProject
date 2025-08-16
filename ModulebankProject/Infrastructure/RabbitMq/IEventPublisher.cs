namespace ModulebankProject.Infrastructure.RabbitMq
{
    public interface IEventPublisher
    {
        Task PublishPendingEventsAsync(CancellationToken cancellationToken);
    }
}
