using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEventEntity>
{
    public void Configure(EntityTypeBuilder<OutboxEventEntity> builder)
    {
        builder.ToTable("tbl_outbox", "events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Payload).HasColumnType("jsonb");
        builder.HasIndex(e => e.ProcessedAt);
    }
}
