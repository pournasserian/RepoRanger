using MongoDB.Bson;
using System.Text.Json;

namespace RepoRanger;

/// <summary>
/// Utility methods for data conversion and transformation.
/// Provides helper methods for working with JSON and BSON documents.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Converts a JSON element to a BSON document for MongoDB storage.
    /// </summary>
    /// <param name="item">JSON element to convert</param>
    /// <returns>BSON document ready for MongoDB storage</returns>
    /// <remarks>
    /// This method handles type conversions and adds additional metadata.
    /// Particularly important for ensuring ID fields are properly typed.
    /// </remarks>
    public static BsonDocument? ConvertToBson(this JsonElement item)
    {
        // Convert the JSON element to a string and then to a BsonDocument
        // This two-step process ensures proper handling of all JSON types
        string itemJson = item.ToString();
        var repoDocument = BsonDocument.Parse(itemJson);

        // Ensure the `id` field is stored as a long (Int64)
        // MongoDB works better with consistent ID types
        SetIdAsLong(repoDocument);

        // Add an internal timestamp for when the document was created
        // This helps with tracking when data was added to our system
        AddInternalCreatedAt(repoDocument);

        return repoDocument;
    }

    /// <summary>
    /// Adds an internal creation timestamp to the BSON document.
    /// </summary>
    /// <param name="item">BSON document to modify</param>
    private static void AddInternalCreatedAt(this BsonDocument item)
    {
        // Add a new field `internal_created_at` with the current UTC date and time
        // This is different from GitHub's timestamps and represents when we indexed the repository
        item.Add("internal_created_at", DateTime.UtcNow);
    }

    /// <summary>
    /// Ensures the ID field is stored as a 64-bit integer in the BSON document.
    /// </summary>
    /// <param name="item">BSON document to modify</param>
    /// <remarks>
    /// GitHub IDs can be large numbers, so we always store them as Int64
    /// even if they come from the API as Int32.
    /// </remarks>
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

    /// <summary>
    /// Adds README content to a BSON document.
    /// </summary>
    /// <param name="item">BSON document to update</param>
    /// <param name="readmeContent">README content to add</param>
    public static void SetReadMeContent(this BsonDocument item, string? readmeContent)
    {
        // If the readme content is null or whitespace, do nothing
        if (string.IsNullOrWhiteSpace(readmeContent))
            return;

        // Add a new field `readme_content` with the provided content
        item.Add("readme_content", readmeContent);
    }

    /// <summary>
    /// Converts a JSON element to a GitHubRepository object.
    /// </summary>
    /// <param name="json">JSON element representing a GitHub repository</param>
    /// <returns>Structured GitHubRepository object</returns>
    /// <remarks>
    /// This provides a clean mapping between the GitHub API JSON response
    /// and our domain model.
    /// </remarks>
    public static GitHubRepository ConvertToGitHubRepository(this JsonElement json)
    {
        return new GitHubRepository
        {
            // Map each JSON property to the corresponding model property
            // Handle potential nulls in string fields
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
}
