using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulebankProject.Features.Accounts;

namespace ModulebankProject.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder
            .HasIndex(a => a.OwnerId)
            .HasMethod("HASH");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.OwnerId)
            .IsRequired();

        builder.Property(a => a.AccountType)
            .IsRequired();

        builder.Property(a => a.Currency)
            .IsRequired();

        builder.Property(a => a.Balance)
            .IsRequired();

        builder.Property(a => a.InterestRate);

        builder.Property(a => a.OpenDate)
            .IsRequired();

        builder.Property(a => a.CloseDate)
            .IsRequired();

        builder.HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId);

        builder
            .Property(t => t.Version)
            .IsConcurrencyToken()
            // ReSharper disable once StringLiteralTypo
            .HasColumnName("xmin")
            .HasColumnType("xid");
    }
}