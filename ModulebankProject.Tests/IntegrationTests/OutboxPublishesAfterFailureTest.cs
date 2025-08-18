using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Outbox;
using ModulebankProject.Infrastructure.RabbitMq;
using ModulebankProject.Tests.Base;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using Testcontainers.RabbitMq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ModulebankProject.Tests.IntegrationTests
{
    public class OutboxPublishesAfterFailureTest : BaseIntegrationTest, IAsyncLifetime
    {
        private readonly RabbitMqContainer _rabbitMqContainer;
        private readonly ITestOutputHelper _testOutputHelper;
        private IConnection _connection;
        private IChannel _channel;
        private ConnectionFactory _connectionFactory;

        private const string ExchangeName = "account.events";
        private const string TestQueueName = "test_queue";

        public OutboxPublishesAfterFailureTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
        {
            _testOutputHelper = testOutputHelper;
            _rabbitMqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.13.7-management")
                .WithUsername("admin")
                .WithPassword("admin")
                .Build();
        }
        [Fact]
        public async Task Should_Publish_Outbox_Messages_After_RabbitMQ_Recovery()
        {
            // Arrange
            var logger = Mock.Of<ILogger<RabbitMqEventPublisher>>();
            var publisher = new RabbitMqEventPublisher(DbContext, _connection, logger);
            await _rabbitMqContainer.StopAsync();

            var messageId = Guid.NewGuid();
            var outboxMessage = new OutboxMessage
            {
                Id = messageId,
                Type = "account.test",
                Content = "123",
                OccurredOn = DateTime.UtcNow
            };
            await DbContext.OutboxMessages.AddAsync(outboxMessage);
            await DbContext.SaveChangesAsync();

            //Act1
            var result1 = await publisher.PublishPendingEventsAsync(CancellationToken.None);
            //Assert1
            Assert.False(result1);

            //Act2
            await _rabbitMqContainer.StartAsync();
            await InitializeRabbitMqInfrastructure();
            publisher = new RabbitMqEventPublisher(DbContext, _connection, logger);

            await Task.Delay(TimeSpan.FromSeconds(10));

            var result2 = await publisher.PublishPendingEventsAsync(CancellationToken.None);
            //Assert2
            Assert.True(result2);

            //Act3
            var message2 = await WaitForMessage(TestQueueName);
            _testOutputHelper.WriteLine("GettedResult = " + message2.Content);
            //Assert3
            Assert.NotNull(message2!.Content);
            Assert.Equal("123", message2.Content);
        }
        private async Task<ReceivedMessage?> WaitForMessage(string queueName, int timeoutSeconds = 10)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var tcs = new TaskCompletionSource<ReceivedMessage>();
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                tcs.TrySetResult(new ReceivedMessage(
                    Encoding.UTF8.GetString(ea.Body.ToArray()),
                    ea.RoutingKey
                ));
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);

            try
            {
                return await tcs.Task.WaitAsync(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return null; // Timeout - no message received
            }
            finally
            {
                await channel.CloseAsync();
            }
        }
        public async Task InitializeAsync()
        {
            await _rabbitMqContainer.StartAsync();

            await InitializeRabbitMqInfrastructure();
        }
        private async Task InitializeRabbitMqInfrastructure()
        {
            _connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(_rabbitMqContainer.GetConnectionString()),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            _connection = await _connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);
            await _channel.QueueDeclareAsync(TestQueueName, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync(TestQueueName, ExchangeName, "account.test");
            await _channel.QueuePurgeAsync(TestQueueName);
        }

        public async Task DisposeAsync()
        {
            await _rabbitMqContainer.DisposeAsync();
        }
    }
    public record ReceivedMessage(string Content, string RoutingKey);
}
