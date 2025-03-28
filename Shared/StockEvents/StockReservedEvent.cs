using MassTransit;
using Shared.Messages;

namespace Shared.StockEvents;

public class StockReservedEvent : CorrelatedBy<Guid>
{
    public StockReservedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public IEnumerable<OrderItemMessage> OrderItems { get; set; }
}