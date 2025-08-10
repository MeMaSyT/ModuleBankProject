using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Tests.Base;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace ModulebankProject.Tests.IntegrationTests
{
    public class ParallelTransferTests : BaseIntegrationTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const string Endpoint = "/api/Transactions/";

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public ParallelTransferTests(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Call50Transactions()
        {
            // Arrange
            //WebApplicationFactory<Program> web = new WebApplicationFactory<Program>().WithWebHostBuilder(_ => { });
            //HttpClient httpClient = web.CreateClient();
            var transactions = new List<Transaction>();
            Guid owner = Guid.NewGuid();
            
            var fromAccount = new Account
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
            var toAccount = new Account
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
            await DbContext.Accounts.AddRangeAsync(fromAccount, toAccount);

            Random rd = new Random();
            for (int i = 0; i < 50; i++)
            {
                var fromId = rd.Next(10) > 4 ? fromAccount.Id : toAccount.Id;
                var toId = fromId == fromAccount.Id ? toAccount.Id : fromAccount.Id;

                var transaction = new Transaction(
                    Guid.NewGuid(),
                    fromId,
                    toId,
                    10M,
                    "rub",
                    TransactionType.Debit,
                    "test",
                    DateTime.UtcNow,
                    TransactionStatus.Registered
                );
                transactions.Add(transaction);
            }
            await DbContext.Transactions.AddRangeAsync(transactions);
            await DbContext.SaveChangesAsync();

            //Act
            var tasks = transactions.Select(transaction =>
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, Endpoint + owner + "/" + transaction.Id)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new {}),
                        Encoding.UTF8,
                        "application/json")
                };
                return Client.SendAsync(request);
            }).ToList();

            var responses = await Task.WhenAll(tasks);
            await DbContext.Entry(fromAccount).ReloadAsync();
            await DbContext.Entry(toAccount).ReloadAsync();
            //Assert
            foreach (var response in responses)
            {
                var content = await response.Content.ReadAsStringAsync();
                _testOutputHelper.WriteLine($"Status: {response.StatusCode}, Content: {content}");
                //response.EnsureSuccessStatusCode();
            }

            decimal sum = 0M;
            var firstAccount = DbContext.Accounts.FirstOrDefault(a => a.Id == fromAccount.Id)!.Balance;
            var secondAccount = DbContext.Accounts.FirstOrDefault(a => a.Id == toAccount.Id)!.Balance;
            sum += firstAccount + secondAccount;
            _testOutputHelper.WriteLine("1) " + firstAccount + " 2)" + secondAccount);
            Assert.Equal(2000, sum);
        }
    }
}
