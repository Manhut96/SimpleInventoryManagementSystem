using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("tbl_orders", "ordering");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        builder.OwnsMany(o => o.Items, itemBuilder =>
        {
            itemBuilder.ToTable("tbl_order_items", "ordering");
            itemBuilder.WithOwner().HasForeignKey("OrderId");
            itemBuilder.HasKey("OrderId", "ProductId");
            itemBuilder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            itemBuilder.Property(i => i.FinalUnitPrice).HasColumnType("decimal(18,2)");
        });
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(o => o.CustomerId);
    }
}
