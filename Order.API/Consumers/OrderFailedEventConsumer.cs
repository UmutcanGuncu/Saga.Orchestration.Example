using MassTransit;
using Order.API.Contexts;
using Order.API.Models.Enums;
using Shared.OrderEvents;

namespace Order.API.Consumers;

public class OrderFailedEventConsumer(OrderAPIDBContext orderApidbContext) : IConsumer<OrderFailedEvent>
{
    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        var order = await orderApidbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = OrderStatus.Failed;
            await orderApidbContext.SaveChangesAsync();
        }
    }
}