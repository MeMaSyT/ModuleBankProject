using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulebankProject.Features.Transactions;

namespace ModulebankProject.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .HasIndex(t => new { t.AccountId, t.Time });

        builder
            .HasIndex(t => t.Time)
            .HasMethod("GIST");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.AccountId)
            .IsRequired();

        builder.Property(t => t.CounterpartyAccountId);

        builder.Property(t => t.Amount)
            .IsRequired();

        builder.Property(t => t.Currency)
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .IsRequired();

        builder.Property(t => t.Description);

        builder.Property(t => t.Time)
            .IsRequired();

        builder.Property(t => t.TransactionStatus)
            .IsRequired();

        builder
            .Property(t => t.Version)
            .IsConcurrencyToken()
            // ReSharper disable once StringLiteralTypo
            .HasColumnName("xmin")
            .HasColumnType("xid");
    }
}