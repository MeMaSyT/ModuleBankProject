using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulebankProject.Features.Inbox;

namespace ModulebankProject.Infrastructure.Data.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder
            .ToTable("inbox_messages");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.ProcessedAt);

        builder
            .Property(x => x.Handler)
            .IsRequired();
    }
}