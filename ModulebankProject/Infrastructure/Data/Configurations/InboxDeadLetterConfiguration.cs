using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulebankProject.Features.InboxDeadLetter;

namespace ModulebankProject.Infrastructure.Data.Configurations;

public class InboxDeadLetterConfiguration : IEntityTypeConfiguration<InboxDeadLetter>
{
    public void Configure(EntityTypeBuilder<InboxDeadLetter> builder)
    {
        builder
            .ToTable("inbox_dead_letters");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.ProcessedAt);

        builder
            .Property(x => x.Handler)
            .IsRequired();

        builder
            .Property(x => x.Payload);

        builder
            .Property(x => x.Error);
    }
}