using ModulebankProject.Infrastructure.RabbitMq;

namespace ModulebankProject.Features.Outbox
{
    public class OutboxBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

        public OutboxBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<OutboxBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                    await publisher.PublishPendingEventsAsync(stoppingToken);
                    Console.WriteLine("AwakePublish!");
                }
                catch (Exception ex)
                {
                   Console.WriteLine("Error processing outbox messages \n" + ex);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
