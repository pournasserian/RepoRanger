# Helpers.cs

## Overview

This file contains utility extension methods that facilitate data conversion and transformation operations in the RepoRanger application. These helper methods primarily handle the conversion between JSON data structures returned by the GitHub API and the application's internal data models.

## Key Components

### Helpers Static Class

```csharp
public static class Helpers
{
    // Extension methods for data conversion and manipulation
}
```

The `Helpers` class is a static utility class that provides extension methods for:
- Converting JSON elements to BSON documents
- Setting README content in BSON documents
- Converting JSON elements to GitHubRepository objects

## Extension Methods

### ConvertToBson

```csharp
public static BsonDocument? ConvertToBson(this JsonElement item)
{
    // Convert the JSON element to a string and then to a BsonDocument
    string itemJson = item.ToString();
    var repoDocument = BsonDocument.Parse(itemJson);

    // Ensure the `id` field is stored as a long (Int64)
    SetIdAsLong(repoDocument);

    // Add an internal timestamp for when the document was created
    AddInternalCreatedAt(repoDocument);

    return repoDocument;
}
```

This extension method:
1. Converts a `JsonElement` to a BSON document by first converting it to a JSON string
2. Ensures the repository ID is stored as a 64-bit integer
3. Adds a timestamp to track when the document was created in the system
4. Returns the resulting BSON document

### AddInternalCreatedAt

```csharp
private static void AddInternalCreatedAt(this BsonDocument item)
{
    // Add a new field `internal_created_at` with the current UTC date and time
    item.Add("internal_created_at", DateTime.UtcNow);
}
```

This private extension method adds an internal timestamp field to a BSON document to track when it was created in the system.

### SetIdAsLong

```csharp
private static void SetIdAsLong(BsonDocument item)
{
    // Ensure `id` is stored as a long (Int64)
    if (item.TryGetElement("id", out BsonElement idElement) && idElement.Value.IsInt32)
    {
        // Convert the `id` value to Int64
        long repoId = idElement.Value.AsInt64;
        // Update the `id` field to be stored as BsonInt64
        item["id"] = new BsonInt64(repoId);
    }
}
```

This private extension method ensures that the repository ID in a BSON document is stored as a 64-bit integer, converting it if necessary.

### SetReadMeContent

```csharp
public static void SetReadMeContent(this BsonDocument item, string? readmeContent)
{
    // If the readme content is null or whitespace, do nothing
    if (string.IsNullOrWhiteSpace(readmeContent))
        return;

    // Add a new field `readme_content` with the provided content
    item.Add("readme_content", readmeContent);
}
```

This extension method adds README content to a BSON document, but only if valid content is provided.

### ConvertToGitHubRepository

```csharp
public static GitHubRepository ConvertToGitHubRepository(this JsonElement json)
{
    return new GitHubRepository
    {
        Id = json.GetProperty("id").GetInt64(),
        Name = json.GetProperty("name").GetString(),
        FullName = json.GetProperty("full_name").GetString(),
        HtmlUrl = json.GetProperty("html_url").GetString(),
        Description = json.GetProperty("description").GetString(),
        CreatedAt = json.GetProperty("created_at").GetDateTime(),
        UpdatedAt = json.GetProperty("updated_at").GetDateTime(),
        PushedAt = json.GetProperty("pushed_at").GetDateTime(),
        Language = json.GetProperty("language").GetString(),
        HomePage = json.GetProperty("homepage").GetString(),
        Size = json.GetProperty("size").GetInt64(),
        StargazersCount = json.GetProperty("stargazers_count").GetInt32(),
        WatchersCount = json.GetProperty("watchers_count").GetInt32(),
        ForksCount = json.GetProperty("forks_count").GetInt32(),
        OpenIssuesCount = json.GetProperty("open_issues_count").GetInt32(),
        Topics = json.GetProperty("topics").EnumerateArray().Select(t => t.GetString()).ToList(),
        DefaultBranch = json.GetProperty("default_branch").GetString(),
        IsTemplate = json.GetProperty("is_template").GetBoolean()
    };
}
```

This extension method converts a JSON element from the GitHub API response to a `GitHubRepository` object by mapping the JSON properties to the corresponding properties in the model.

## Dependencies

- MongoDB.Bson (for BSON document manipulation)
- System.Text.Json (for JSON parsing)
- RepoRanger.GitHubRepository (model class for GitHub repositories)

## Usage

These helper methods are used throughout the application to:

1. Convert API responses to model objects:
   ```csharp
   GitHubRepository repo = jsonElement.ConvertToGitHubRepository();
   ```

2. Prepare data for MongoDB storage:
   ```csharp
   BsonDocument doc = jsonElement.ConvertToBson();
   ```

3. Add README content to repository documents:
   ```csharp
   bsonDocument.SetReadMeContent(readmeContent);
   ```
