using MongoDB.Bson;
using System.Text.Json;

namespace RepoRanger;

public static class Helpers
{
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

    private static void AddInternalCreatedAt(this BsonDocument item)
    {
        // Add a new field `internal_created_at` with the current UTC date and time
        item.Add("internal_created_at", DateTime.UtcNow);
    }

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

    public static void SetReadMeContent(this BsonDocument item, string? readmeContent)
    {
        // If the readme content is null or whitespace, do nothing
        if (string.IsNullOrWhiteSpace(readmeContent))
            return;

        // Add a new field `readme_content` with the provided content
        item.Add("readme_content", readmeContent);
    }

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

}
