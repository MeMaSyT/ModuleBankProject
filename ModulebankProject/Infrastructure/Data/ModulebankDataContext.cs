using Microsoft.EntityFrameworkCore;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Infrastructure.Data.Configurations;

namespace ModulebankProject.Infrastructure.Data
{
    public class ModulebankDataContext(DbContextOptions<ModulebankDataContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }
}
