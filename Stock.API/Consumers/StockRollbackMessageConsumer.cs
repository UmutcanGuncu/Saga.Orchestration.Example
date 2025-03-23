using MassTransit;
using Shared.Messages;
using Stock.API.Services;
using MongoDB.Driver;

namespace Stock.API.Consumers;

public class StockRollbackMessageConsumer(MongoDBService mongoDbService) : IConsumer<StockRollbackMessage>
{
    public async Task Consume(ConsumeContext<StockRollbackMessage> context)
    {
        var stockCollection = mongoDbService.GetCollection<Models.Stock>();
        foreach (var orderItem in context.Message.OrderItems)
        {
             var stock = await (await stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
             stock.Count += orderItem.Count;
             await stockCollection.FindOneAndReplaceAsync(x=> x.ProductId == orderItem.ProductId, stock);
        }
    }
}