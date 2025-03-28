using MassTransit;
using Shared.Messages;

namespace Shared.OrderEvents;

public class OrderCreatedEvent : CorrelatedBy<Guid>
{
    public OrderCreatedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public IEnumerable<OrderItemMessage> OrderItems { get; set; }
}