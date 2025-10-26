

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Core.Entities;

namespace OrderService.Infrastructure.Configs.OrderConfigs
{
    public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("OrderItemID")
                .HasColumnType("uuid");

            builder.Property(e => e.OrderId)
                .HasColumnName("OrderID")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(e => e.ProductId)
                .HasColumnName("ProductID")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(e => e.Quantity)
                .HasColumnType("integer")
                .IsRequired();

            builder.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10,2)");

            builder.HasOne(d => d.Order)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}