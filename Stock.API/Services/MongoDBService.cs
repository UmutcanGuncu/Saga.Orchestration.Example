using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDBService
{
    private readonly IMongoDatabase _database;

    public MongoDBService(IConfiguration configuration)
    {
        MongoClient client = new MongoClient(configuration.GetConnectionString("MongoDB"));
        _database = client.GetDatabase("OrchestrationStockDB");
    }
    public IMongoCollection<T> GetCollection<T>() => 
        _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
}