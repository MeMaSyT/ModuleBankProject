using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Outbox;
using ModulebankProject.Infrastructure.Data;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

namespace ModulebankProject.Infrastructure.RabbitMq
{
    public sealed class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly ModulebankDataContext _dbContext;
        private readonly IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<RabbitMqEventPublisher> _logger;

        public RabbitMqEventPublisher(
            ModulebankDataContext dbContext,
            IConnection connection,
            ILogger<RabbitMqEventPublisher> logger)
        {
            _dbContext = dbContext;
            _connection = connection;
            _logger = logger;
        }

        public async Task<bool> PublishPendingEventsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                return false;
            }
            

            var messages = await _dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedOn == null)
                .OrderBy(m => m.OccurredOn)
                .Take(100)
                .ToListAsync(cancellationToken);
            var counter = 0;

            foreach (var message in messages)
            {
                Console.WriteLine("COntent = " + message.Content);
                var props = new BasicProperties();
                if (message.Properties != null)
                {
                    if(message.Properties.TryGetValue("priority", out var type)) props.Type = type.ToString();
                }
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var body = Encoding.UTF8.GetBytes(message.Content);
                    await _channel.BasicPublishAsync(
                        exchange: "account.events",
                        routingKey: message.Type,
                        mandatory: true,
                        basicProperties: props,
                        body: body,
                        cancellationToken);

                    message.ProcessedOn = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Message published {@MessageInfo}", new
                    {
                        EventId = Guid.NewGuid(),
                        Type = "AccountOpenedEvent",
                        CorrelationId = message.Id,
                        RoutingKey = "account.opened",
                        LatencyMs = stopwatch.ElapsedMilliseconds,
                        MessageSize = body.Length
                    });
                    counter++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish message {@MessageInfo}", new
                    {
                        EventId = message.Id,
                        Type = message.Type,
                        CorrelationId = message.Id,
                        RetryCount = 0
                    });
                    message.Error = ex.Message;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            return messages.Count == counter ? true : false;
        }
    }
}
