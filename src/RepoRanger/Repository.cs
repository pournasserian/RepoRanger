using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;


namespace RepoRanger;

public interface IRepository
{
    Task InsertManyAsync(IEnumerable<BsonDocument> repositories);
}

public class MongoDbRepository : IRepository
{
    private readonly IMongoCollection<BsonDocument> _repositoriesCollection;
    private readonly MongoClient _client;

    public MongoDbRepository(IConfiguration configuration)
    {
        var mongoConnectionString = configuration["MongoDbSettings:ConnectionString"];
        var mongoDatabaseName = configuration["MongoDbSettings:DatabaseName"];
        var mongoCollectionName = configuration["MongoDbSettings:CollectionName"];

        if (string.IsNullOrEmpty(mongoConnectionString) || string.IsNullOrEmpty(mongoDatabaseName) || string.IsNullOrEmpty(mongoCollectionName))
            throw new Exception("Error: MongoDB settings not properly configured in appsettings.json");

        _client = new MongoClient(mongoConnectionString);
        _repositoriesCollection = _client.GetDatabase(mongoDatabaseName).GetCollection<BsonDocument>(mongoCollectionName);
    }

    public async Task InsertManyAsync(IEnumerable<BsonDocument> items)
    {
        try
        {
            // Prepare bulk write operations
            var bulkOps = new List<WriteModel<BsonDocument>>();

            foreach (var item in items)
            {
                // Create a filter based on the repository id
                var filter = Builders<BsonDocument>.Filter.Eq("id", item["id"].AsInt64);

                // Create an upsert operation
                var upsert = new ReplaceOneModel<BsonDocument>(filter, item) { IsUpsert = true };
                bulkOps.Add(upsert);
            }

            if (bulkOps.Count > 0)
            {
                var result = await _repositoriesCollection.BulkWriteAsync(bulkOps);
                Console.WriteLine($"MongoDB: Inserted {result.InsertedCount}, Updated {result.ModifiedCount} documents");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error inserting repositories: {ex.Message}");
        }
    }
}
