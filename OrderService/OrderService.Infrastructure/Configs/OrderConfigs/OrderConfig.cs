// BagsStore.Infrastructure.Configs/OrderConfig.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Core.Entities;

namespace OrderService.Infrastructure.Configs.OrderConfigs
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnName("OrderID")
                   .HasColumnType("uuid");

            builder.Property(e => e.UserId)
                   .HasColumnName("UserID")
                   .HasColumnType("uuid");

            builder.Property(e => e.TrackingId)
                   .HasMaxLength(255)
                   .HasColumnType("text")
                   .IsRequired();

            builder.Property(e => e.Address)
                   .HasColumnType("text");

            builder.Property(e => e.City)
                   .HasColumnType("text");

            builder.Property(e => e.Zip)
                   .HasColumnType("integer")
                   .IsRequired(false);

            builder.Property(e => e.OrderDate)
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.TotalAmount)
                   .HasColumnType("decimal(10,2)");

            builder.Property(e => e.Status)
                   .HasColumnType("text")
                   .HasMaxLength(50);
        }
    }
}
