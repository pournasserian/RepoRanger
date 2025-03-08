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
}
