namespace Shared.Messages;

public class StockRollbackMessage
{
    public IEnumerable<OrderItemMessage> OrderItems { get; set; }
}