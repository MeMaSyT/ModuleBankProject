using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Infrastructure.Data;
using ModulebankProject.Infrastructure.Data.Repositories;
using Moq;
using Testcontainers.PostgreSql;

namespace ModulebankProject.Tests.ModuleTests;

public class AccountRepositoryTests : IAsyncLifetime
{
    // ReSharper disable once IdentifierTypo
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private ModulebankDataContext _dbContext;
    private IAccountsRepository _accountRepository;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable. В данном случае мне не нужен DI
    public AccountRepositoryTests()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("123")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<ModulebankDataContext>()
            .UseNpgsql(connectionString)
            .Options;

        _dbContext = new ModulebankDataContext(options);
        await _dbContext.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS btree_gist;");
        await _dbContext.Database.MigrateAsync();

        var mockMapper = new Mock<IMapper>();
        _accountRepository = new AccountsRepository(_dbContext, mockMapper.Object);
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task TransferMoney_ShouldCorrectlyUpdateBalances()
    {
        // Arrange
        var fromAccount = new Account
        (
            Guid.NewGuid(),
            Guid.NewGuid(),
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
            Guid.NewGuid(),
            AccountType.Checking,
            "rub",
            1000M,
            5,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1)
        );
        var fromTransaction = new Transaction(
            Guid.NewGuid(),
            fromAccount.Id,
            toAccount.Id,
            1000M,
            "rub",
            TransactionType.Debit,
            "test",
            DateTime.UtcNow, 
            TransactionStatus.Registered
        );
        var toTransaction = new Transaction(
            Guid.NewGuid(),
            toAccount.Id,
            fromAccount.Id,
            1000M,
            "rub",
            TransactionType.Credit,
            "test",
            DateTime.UtcNow,
            TransactionStatus.Registered
        );

        await _dbContext.Accounts.AddRangeAsync(fromAccount, toAccount);
        await _dbContext.Transactions.AddRangeAsync(fromTransaction, toTransaction);
        await _dbContext.SaveChangesAsync();

        // Act
        await _accountRepository.ApplyTransaction(fromTransaction, toTransaction);

        // Assert
        var updatedFromAccount = await _dbContext.Accounts.FindAsync(fromAccount.Id);
        var updatedToAccount = await _dbContext.Accounts.FindAsync(toAccount.Id);

        Assert.Equal(2000M, updatedFromAccount!.Balance);
        Assert.Equal(0M, updatedToAccount!.Balance);
    }
}