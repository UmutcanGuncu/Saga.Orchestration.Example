using MassTransit;
using Shared.Messages;

namespace Shared.PaymentEvents;

public class PaymentStartedEvent : CorrelatedBy<Guid>
{
    public PaymentStartedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public decimal TotalPrice { get; set; }
    public IEnumerable<OrderItemMessage> OrderItems { get; set; }
}