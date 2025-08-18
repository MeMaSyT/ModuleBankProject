using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Inbox;
using ModulebankProject.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ModulebankProject.Features.Inbox.Events;
using ModulebankProject.Features.InboxDeadLetter;

namespace ModulebankProject.Infrastructure.RabbitMq.AntifraudConsumer;

public class AntifraudConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private const string QueueName = "account.antifraud";

    private readonly IAntifraudEventHandler _handler;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AntifraudConsumer(
        IChannel channel,
        IServiceProvider serviceProvider, ILogger<RabbitMqEventPublisher> logger, IAntifraudEventHandler handler)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _handler = handler;

        _channel = channel;
        /*
        _channel.QueueDeclareAsync(queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        */
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        Console.WriteLine("Consumer is listening");

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var eventType = ea.BasicProperties.Type;
            var stopwatch = Stopwatch.StartNew();
            var correlationId = ea.BasicProperties.CorrelationId;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ModulebankDataContext>();

                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<ClientChangeBlockEvent>(messageJson);

                var inboxMessage = new InboxMessage
                {
                    Id = message!.EventId,
                    Handler = "AntifraudEventHandler",
                    Payload = message.ClientId.ToString()
                };

                //START VALIDATION
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse так лучше для читабельности
                if (message is not { Version: "v1" })
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    await QuarantineMessageAsync(dbContext, "Message is null", inboxMessage);
                    _logger.LogError("Message is null");
                    return;
                }
                Console.WriteLine(message.ClientId + "/--------------------------------------------");
                if (await dbContext.InboxMessages.AnyAsync(m => m.Id == inboxMessage.Id, stoppingToken))
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    await QuarantineMessageAsync(dbContext, "Message already has on DB", inboxMessage);
                    _logger.LogError("Message already has on DB");
                    return;
                }
                inboxMessage.ProcessedAt = DateTime.UtcNow;
                dbContext.InboxMessages.Add(inboxMessage);
                await dbContext.SaveChangesAsync(stoppingToken);

                var isFreeze = false;
                switch (eventType)
                {
                    case "ClientBlockEvent":
                        isFreeze = true;
                        break;
                    case "ClientUnblockedEvent":
                        isFreeze = false;
                        break;
                    default:
                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        await QuarantineMessageAsync(dbContext, $"EventType {eventType} not found", inboxMessage);
                        _logger.LogError($"EventType {eventType} not found");
                        break;
                }
                await _handler.HandleAsync(message.ClientId, isFreeze, stoppingToken);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

                _logger.LogInformation("Message processed {@ProcessingInfo}", new
                {
                    message.EventId,
                    CorrelationId = correlationId,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    Status = "Success"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message processing failed {@ProcessingInfo}", new
                {
                    CorrelationId = correlationId,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    Status = "Failed",
                    RetryCount = 0
                });
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken); // Повторяем попытку
            }
        };

        await _channel.BasicConsumeAsync(queue: QueueName,
            autoAck: false,
            consumer: consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
    private static async Task QuarantineMessageAsync(
        ModulebankDataContext dbContext,
        string error, InboxMessage message)
    {
        var deadLetter = new InboxDeadLetter
        {
            Id = Guid.NewGuid(),
            Handler = message.Handler,
            Payload = message.Payload!,
            Error = error
        };

        await dbContext.InboxDeadLetters.AddAsync(deadLetter);
        await dbContext.SaveChangesAsync();
    }
}