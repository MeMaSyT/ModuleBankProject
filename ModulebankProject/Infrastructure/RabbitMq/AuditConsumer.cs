using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Inbox;
using ModulebankProject.Features.InboxDeadLetter;
using ModulebankProject.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ModulebankProject.Infrastructure.RabbitMq;

public class AuditConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private const string QueueName = "account.audit";

    // ReSharper disable once ConvertToPrimaryConstructor
    public AuditConsumer(
        IChannel channel,
        IServiceProvider serviceProvider, ILogger<RabbitMqEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

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
        Console.WriteLine("Audit Consumer is listening");

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = ea.BasicProperties.CorrelationId;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ModulebankDataContext>();

                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<InboxMessage>(messageJson);

                //START VALIDATION
                if (message == null)
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    await QuarantineMessageAsync(dbContext, "Message is null", message!);
                    _logger.LogError("Message is null");
                    return;
                }

                if (await dbContext.InboxMessages.AnyAsync(m => m.Id == message.Id, stoppingToken))
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    await QuarantineMessageAsync(dbContext, "Message already has on DB", message);
                    _logger.LogError("Message already has on DB");
                    return;
                }
                message.ProcessedAt = DateTime.UtcNow;
                dbContext.InboxMessages.Add(message);
                await dbContext.SaveChangesAsync(stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

                _logger.LogInformation("Message processed {@ProcessingInfo}", new
                {
                    EventId = message.Id,
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