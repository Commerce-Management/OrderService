using OrderService.Shared.Enums;

namespace OrderService.Core.Entities;

public class PaymentTransaction : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; }
    public string TransactionReference { get; set; } = Guid.NewGuid().ToString();
    public virtual Order? Order { get; set; }
}