

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Core.Entities;

namespace OrderService.Infrastructure.Configs.OrderConfigs
{
    public class PaymentTransactionConfig : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.HasKey(pt => pt.Id);  

            builder.Property(pt => pt.Amount)  
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(pt => pt.TransactionDate)  
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            builder.Property(pt => pt.Status) 
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(pt => pt.TransactionReference)  
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pt => pt.OrderId) 
                .IsRequired(false)
                .HasColumnType("uuid");


            // При удалении Order транзакция остаётся: Restrict  (без изменений)
            builder.HasOne(pt => pt.Order)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            
        }
    }
}