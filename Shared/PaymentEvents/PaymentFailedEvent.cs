using MassTransit;
using Shared.Messages;

namespace Shared.PaymentEvents;

public class PaymentFailedEvent : CorrelatedBy<Guid>
{
    public PaymentFailedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public string Message { get; set; }
    public IEnumerable<OrderItemMessage> OrderItems { get; set; }
}