using OrderService.Shared.Enums;

namespace OrderService.Core.Entities;

public partial class Order : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TrackingId { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public int? Zip { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public virtual OrderStatus Status { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}