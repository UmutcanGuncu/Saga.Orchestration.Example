using MassTransit;
using Shared.Messages;

namespace Shared.PaymentEvents;

public class PaymentCompletedEvent : CorrelatedBy<Guid>
{
    public PaymentCompletedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
}