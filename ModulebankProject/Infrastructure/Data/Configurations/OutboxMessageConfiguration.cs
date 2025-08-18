using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulebankProject.Features.Outbox;

namespace ModulebankProject.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder
            .ToTable("outbox_messages");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(255);

        builder
            .Property(x => x.Content)
            .IsRequired();

        builder
            .Property(x => x.OccurredOn)
            .IsRequired();

        builder
            .Property(x => x.ProcessedOn);

        builder
            .Property(x => x.Error);
    }
}