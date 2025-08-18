using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Outbox;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Features.Transactions.TransferTransaction;
using ModulebankProject.Infrastructure.RabbitMq;
using ModulebankProject.Infrastructure.RabbitMq.AntifraudConsumer;
using ModulebankProject.MbResult;
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
    public class ClientBlockedPreventsDebit : BaseIntegrationTest
    {
        private readonly RabbitMqContainer _rabbitMqContainer;
        private readonly ITestOutputHelper _testOutputHelper;
        private IConnection _connection;
        private IChannel _channel;
        private ConnectionFactory _connectionFactory;

        private const string ExchangeName = "account.events";
        private const string TestQueueName = "test_queue";
        public ClientBlockedPreventsDebit(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper)
            : base(factory)
        {
            _testOutputHelper = testOutputHelper;
            _rabbitMqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.13.7-management")
                .WithUsername("admin")
                .WithPassword("admin")
                .Build();
        }

        [Fact]
        public async Task Transaction_Return_Conflict_After_Freeze_Account()
        {
            //Arrange
            await _rabbitMqContainer.StartAsync();

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

            Guid owner = Guid.NewGuid();
            var account = new Account
            (
                Guid.NewGuid(),
                owner,
                AccountType.Checking,
                "rub",
                1000M,
                5,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1)
            );
            await DbContext.Accounts.AddAsync(account);
            await DbContext.SaveChangesAsync();
            var transaction = new Transaction(
                    Guid.NewGuid(),
                    account.Id,
                    null,
                    10M,
                    "rub",
                    TransactionType.Debit,
                    "test",
                    DateTime.UtcNow,
                    TransactionStatus.Registered
                );

            var logger = Mock.Of<ILogger<RabbitMqEventPublisher>>();
            var antifraudLogger = Mock.Of<ILogger<AntifraudEventHandler>>();
            var publisher = new RabbitMqEventPublisher(DbContext, _connection, logger);
            var mediatorMock = new Mock<IMediator>();

            var messageId = Guid.NewGuid();
            var outboxMessage = new OutboxMessage
            {
                Id = messageId,
                Type = "account.test",
                Content = owner.ToString(),
                OccurredOn = DateTime.UtcNow
            };
            await DbContext.OutboxMessages.AddAsync(outboxMessage);
            await DbContext.SaveChangesAsync();

            TransferTransactionCommand command = new TransferTransactionCommand(transaction.Id);
            var expectedResult = MbResult<string, ApiError>.Failure(new ApiError("AccountIsFreezing", StatusCodes.Status409Conflict));
            mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            //Act
            await publisher.PublishPendingEventsAsync(CancellationToken.None);
            var message = await WaitForMessage(TestQueueName);
            Assert.Equal(owner.ToString(), message!.Content);

            var antifraudEventHandler = new AntifraudEventHandler(DbContext, antifraudLogger);
            await antifraudEventHandler.HandleAsync(new Guid(message.Content), true, CancellationToken.None);

            var result = await mediatorMock.Object.Send(command);
            _testOutputHelper.WriteLine("result = " + result.Error);
            Assert.Equal(expectedResult, result);
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
    }
}
