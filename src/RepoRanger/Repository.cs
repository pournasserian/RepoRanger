using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;


namespace RepoRanger;

public interface IRepository
{
    Task InsertManyAsync(IEnumerable<BsonDocument> repositories);
    Task InsertManyAsync(IEnumerable<GitHubRepository> repositories);
    Task UpdateReadMeAsync(long repositoryId, string readmeContent);
}

public class MongoDbRepository : IRepository
{
    private readonly IMongoCollection<BsonDocument> _repositoriesCollection;
    private readonly IMongoCollection<GitHubRepository> _githubCollection;
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
        _githubCollection = _client.GetDatabase(mongoDatabaseName).GetCollection<GitHubRepository>(mongoCollectionName);
    }

    public async Task InsertManyAsync(IEnumerable<GitHubRepository> items)
    {
        try
        {
            // Prepare bulk write operations
            var bulkOps = new List<WriteModel<GitHubRepository>>();

            foreach (var item in items)
            {
                // Create a filter based on the repository id
                var filter = Builders<GitHubRepository>.Filter.Eq("id", item.Id);

                // Create an upsert operation
                var upsert = new ReplaceOneModel<GitHubRepository>(filter, item) { IsUpsert = true };
                bulkOps.Add(upsert);
            }

            if (bulkOps.Count > 0)
            {
                var result = await _githubCollection.BulkWriteAsync(bulkOps);
                Console.WriteLine($"MongoDB: Inserted {result.InsertedCount}, Updated {result.ModifiedCount} documents");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error inserting repositories: {ex.Message}");
        }
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

    public async Task UpdateReadMeAsync(long repositoryId, string readmeContent)
    {
        try
        {
            var filter = Builders<GitHubRepository>.Filter.Eq(x => x.Id, repositoryId);
            var update = Builders<GitHubRepository>.Update.Set(x => x.ReadMeContent, readmeContent);
            await _githubCollection.UpdateOneAsync(filter, update);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating readme content: {ex.Message}");
        }
    }
}
