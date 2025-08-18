namespace ModulebankProject.Infrastructure.RabbitMq
{
    public interface IEventPublisher
    {
        Task<bool> PublishPendingEventsAsync(CancellationToken cancellationToken);
    }
}
