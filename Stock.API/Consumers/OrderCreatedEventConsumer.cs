using MassTransit;
using MongoDB.Driver;
using Shared.OrderEvents;
using Shared.Settings;
using Shared.StockEvents;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer(MongoDBService mongoDbService, ISendEndpointProvider sendEndpointProvider) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResults = new();
        var stockCollections =mongoDbService.GetCollection<Models.Stock>();
        foreach (var orderItem in context.Message.OrderItems)
        {
            stockResults.Add(await (await stockCollections.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= (long)orderItem.Count)).AnyAsync());
        }

        var sendEnpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSetting.StateMachineQueue}"));
        if (stockResults.TrueForAll(s => s.Equals(true)))
        {
            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await (await stockCollections.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                stock.Count -= orderItem.Count;
                await stockCollections.FindOneAndReplaceAsync(x=>x.ProductId == orderItem.ProductId, stock);
            }

            StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
            {
                OrderItems = context.Message.OrderItems
            };
            await sendEnpoint.Send(stockReservedEvent);
        }
        else
        {
            StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
            {
                Message = "Elimizde yeterli stok bulunmamaktadır"
            };
            await sendEnpoint.Send(stockNotReservedEvent);
        }
    }
}