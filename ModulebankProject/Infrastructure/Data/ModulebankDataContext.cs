using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Inbox;
using ModulebankProject.Features.InboxDeadLetter;
using ModulebankProject.Features.Outbox;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Infrastructure.Data.Configurations;

namespace ModulebankProject.Infrastructure.Data;

public class ModulebankDataContext(DbContextOptions<ModulebankDataContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxDeadLetterConfiguration());

        base.OnModelCreating(modelBuilder);
    }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxDeadLetter> InboxDeadLetters { get; set; }
}