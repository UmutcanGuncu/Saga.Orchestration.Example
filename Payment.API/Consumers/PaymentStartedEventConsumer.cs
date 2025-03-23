using MassTransit;
using Shared.PaymentEvents;
using Shared.Settings;

namespace Payment.API.Consumers;

public class PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider) : IConsumer<PaymentStartedEvent>
{
    public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
    {
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSetting.StateMachineQueue}"));
        if (true)
        {
            PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId);
            await sendEndpoint.Send<PaymentCompletedEvent>(paymentCompletedEvent);
        }
        else
        {
            PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
            {
                Message = "Payment Failed",
                OrderItems = context.Message.OrderItems
            };
            await sendEndpoint.Send<PaymentFailedEvent>(paymentFailedEvent);
        }
    }
}