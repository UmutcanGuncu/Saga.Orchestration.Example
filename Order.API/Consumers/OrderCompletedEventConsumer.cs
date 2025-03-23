using MassTransit;
using Order.API.Contexts;
using Order.API.Models.Enums;
using Shared.OrderEvents;

namespace Order.API.Consumers;

public class OrderCompletedEventConsumer(OrderAPIDBContext orderApidbContext) : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
       var order = await orderApidbContext.Orders.FindAsync(context.Message.OrderId);
       if (order != null)
       {
           order.Status = OrderStatus.Completed;
           await orderApidbContext.SaveChangesAsync();
       }
    }
}