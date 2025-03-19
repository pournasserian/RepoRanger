using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace RepoRanger;

/// <summary>
/// Interface for repository data storage operations.
/// Defines methods for storing and updating GitHub repository data.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Insert or update multiple repositories as BSON documents.
    /// </summary>
    Task InsertManyAsync(IEnumerable<BsonDocument> repositories);

    /// <summary>
    /// Insert or update multiple repositories as GitHubRepository objects.
    /// </summary>
    Task InsertManyAsync(IEnumerable<GitHubRepository> repositories);

    /// <summary>
    /// Update the README content for a specific repository.
    /// </summary>
    Task UpdateReadMeAsync(long repositoryId, string readmeContent);
}

/// <summary>
/// MongoDB implementation of the repository interface.
/// Handles storing GitHub repository data in MongoDB.
/// </summary>
public class MongoDbRepository : IRepository
{
    private readonly IMongoCollection<BsonDocument> _repositoriesCollection;
    private readonly IMongoCollection<GitHubRepository> _githubCollection;
    private readonly MongoClient _client;

    /// <summary>
    /// Initialize the MongoDB repository with configuration settings.
    /// </summary>
    /// <param name="configuration">Application configuration with MongoDB settings</param>
    public MongoDbRepository(IConfiguration configuration)
    {
        // Get MongoDB connection settings from configuration
        var mongoConnectionString = configuration["MongoDbSettings:ConnectionString"];
        var mongoDatabaseName = configuration["MongoDbSettings:DatabaseName"];
        var mongoCollectionName = configuration["MongoDbSettings:CollectionName"];

        // Validate MongoDB settings
        if (string.IsNullOrEmpty(mongoConnectionString) || string.IsNullOrEmpty(mongoDatabaseName) || string.IsNullOrEmpty(mongoCollectionName))
            throw new Exception("Error: MongoDB settings not properly configured in appsettings.json");

        // Initialize MongoDB client and collections
        // We create two collection references for the same collection:
        // 1. A BsonDocument collection for raw BSON storage
        // 2. A strongly-typed GitHubRepository collection for typed operations
        _client = new MongoClient(mongoConnectionString);
        _repositoriesCollection = _client.GetDatabase(mongoDatabaseName).GetCollection<BsonDocument>(mongoCollectionName);
        _githubCollection = _client.GetDatabase(mongoDatabaseName).GetCollection<GitHubRepository>(mongoCollectionName);
    }

    /// <summary>
    /// Insert or update multiple GitHubRepository objects.
    /// </summary>
    /// <param name="items">Collection of GitHubRepository objects to insert or update</param>
    /// <remarks>
    /// Uses MongoDB's bulk write operations with upsert semantics to efficiently
    /// insert new repositories or update existing ones based on their ID.
    /// </remarks>
    public async Task InsertManyAsync(IEnumerable<GitHubRepository> items)
    {
        try
        {
            // Prepare bulk write operations for better performance
            // This is much more efficient than individual inserts
            var bulkOps = new List<WriteModel<GitHubRepository>>();

            foreach (var item in items)
            {
                // Create a filter to find existing repositories by id
                var filter = Builders<GitHubRepository>.Filter.Eq("id", item.Id);

                // Create an upsert operation (update if exists, insert if not)
                // This ensures we don't create duplicates
                var upsert = new ReplaceOneModel<GitHubRepository>(filter, item) { IsUpsert = true };
                bulkOps.Add(upsert);
            }

            // Execute the bulk operations if we have any
            if (bulkOps.Count > 0)
            {
                var result = await _githubCollection.BulkWriteAsync(bulkOps);
                Console.WriteLine($"MongoDB: Inserted {result.InsertedCount}, Updated {result.ModifiedCount} documents");
            }
        }
        catch (Exception ex)
        {
            // Log errors but don't crash the application
            Console.WriteLine($"❌ Error inserting repositories: {ex.Message}");
        }
    }

    /// <summary>
    /// Insert or update multiple repositories as BSON documents.
    /// </summary>
    /// <param name="items">Collection of BSON documents to insert or update</param>
    /// <remarks>
    /// This is similar to the typed version but works with raw BSON documents.
    /// Useful when working directly with JSON data from the GitHub API.
    /// </remarks>
    public async Task InsertManyAsync(IEnumerable<BsonDocument> items)
    {
        try
        {
            // Prepare bulk write operations for better performance
            var bulkOps = new List<WriteModel<BsonDocument>>();

            foreach (var item in items)
            {
                // Create a filter based on the repository id
                // Note: We need to extract the id value from the BSON document
                var filter = Builders<BsonDocument>.Filter.Eq("id", item["id"].AsInt64);

                // Create an upsert operation (update if exists, insert if not)
                var upsert = new ReplaceOneModel<BsonDocument>(filter, item) { IsUpsert = true };
                bulkOps.Add(upsert);
            }

            // Execute the bulk operations if we have any
            if (bulkOps.Count > 0)
            {
                var result = await _repositoriesCollection.BulkWriteAsync(bulkOps);
                Console.WriteLine($"MongoDB: Inserted {result.InsertedCount}, Updated {result.ModifiedCount} documents");
            }
        }
        catch (Exception ex)
        {
            // Log errors but don't crash the application
            Console.WriteLine($"❌ Error inserting repositories: {ex.Message}");
        }
    }

    /// <summary>
    /// Update the README content for a specific repository.
    /// </summary>
    /// <param name="repositoryId">ID of the repository to update</param>
    /// <param name="readmeContent">README content to add to the repository</param>
    public async Task UpdateReadMeAsync(long repositoryId, string readmeContent)
    {
        try
        {
            // Create a filter to find the repository by its ID
            var filter = Builders<GitHubRepository>.Filter.Eq(x => x.Id, repositoryId);

            // Create an update to set the README content field
            var update = Builders<GitHubRepository>.Update.Set(x => x.ReadMeContent, readmeContent);

            // Apply the update to the repository
            await _githubCollection.UpdateOneAsync(filter, update);
        }
        catch (Exception ex)
        {
            // Log errors but don't crash the application
            Console.WriteLine($"❌ Error updating readme content: {ex.Message}");
        }
    }
}
