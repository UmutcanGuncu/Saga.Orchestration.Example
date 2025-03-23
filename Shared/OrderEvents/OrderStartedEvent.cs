using Shared.Messages;

namespace Shared.OrderEvents;

public class OrderStartedEvent
{
    public int BuyerId { get; set; }
    public int OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public IEnumerable<OrderItemMessage> OrderItems { get; set; }
}