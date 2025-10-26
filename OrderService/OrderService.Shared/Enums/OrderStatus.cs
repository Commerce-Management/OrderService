namespace OrderService.Shared.Enums;

public enum OrderStatus
{
    Pending,      // Ожидает обработки
    Confirmed,    // Подтвержден
    Rejected,     // Отклонен
    OnHold,       // На удержании
    Shipped,      // Отправлен
    Delivered,    // Доставлен
    Cancelled,    // Отменен
    Returned,     // Возвращен
    PaymentFailed // Ошибка при оплате
}