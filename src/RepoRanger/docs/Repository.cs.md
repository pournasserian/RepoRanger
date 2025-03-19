# Repository.cs

## Overview

This file defines the data access layer for the RepoRanger application, providing an abstraction for storing and retrieving GitHub repository data in MongoDB. It includes both the `IRepository` interface that defines the contract for repository operations and its concrete implementation, `MongoDbRepository`.

## Components

### IRepository Interface

```csharp
public interface IRepository
{
    Task InsertManyAsync(IEnumerable<BsonDocument> repositories);
    Task InsertManyAsync(IEnumerable<GitHubRepository> repositories);
    Task UpdateReadMeAsync(long repositoryId, string readmeContent);
}
```

This interface defines the contract for repository operations:

| Method | Parameters | Description |
|--------|------------|-------------|
| `InsertManyAsync` | `IEnumerable<BsonDocument> repositories` | Inserts or updates multiple repositories as BsonDocuments |
| `InsertManyAsync` | `IEnumerable<GitHubRepository> repositories` | Inserts or updates multiple repositories as GitHubRepository objects |
| `UpdateReadMeAsync` | `long repositoryId, string readmeContent` | Updates the README content for a specific repository |

### MongoDbRepository Class

This class implements the `IRepository` interface using MongoDB as the storage mechanism.

#### Constructor

```csharp
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
```

The constructor:
1. Retrieves MongoDB connection settings from the provided configuration
2. Validates that all required settings are present
3. Initializes the MongoDB client and collections

#### InsertManyAsync (GitHubRepository)

```csharp
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
```

This method:
1. Creates a list of bulk write operations, one for each repository
2. For each repository, creates an upsert operation (insert if not exists, update if exists)
3. Executes the bulk write operations and logs the result
4. Handles any exceptions that occur during the process

#### InsertManyAsync (BsonDocument)

```csharp
public async Task InsertManyAsync(IEnumerable<BsonDocument> items)
{
    // Implementation similar to the GitHubRepository version
}
```

This method follows the same pattern as the GitHubRepository version but works with BsonDocument objects.

#### UpdateReadMeAsync

```csharp
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
```

This method:
1. Creates a filter to find the repository by ID
2. Creates an update definition to set the README content
3. Applies the update to the repository in the database
4. Handles any exceptions that occur during the process

## Dependencies

- Microsoft.Extensions.Configuration (for accessing configuration)
- MongoDB.Bson (for working with BSON documents)
- MongoDB.Driver (for interacting with MongoDB)
- RepoRanger.GitHubRepository (model class for GitHub repositories)

## Configuration Requirements

The following settings must be defined in the appsettings.json file:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "reporanger",
    "CollectionName": "repositories"
  }
}
```
