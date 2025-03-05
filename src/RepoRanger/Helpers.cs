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

        SetIdAsLong(repoDocument);
        AddInternalCreatedAt(repoDocument);

        return repoDocument;
    }

    private static void AddInternalCreatedAt(this BsonDocument item)
    {
        item.Add("internal_created_at", DateTime.UtcNow);
    }

    private static void SetIdAsLong(BsonDocument item)
    {
        // Ensure `id` is stored as a long (Int64)
        if (item.TryGetElement("id", out BsonElement idElement) && idElement.Value.IsInt32)
        {
            long repoId = idElement.Value.AsInt32; // Explicitly retrieve as long
            item["id"] = new BsonInt64(repoId); // Ensure it's stored as BsonInt64
        }
    }

    public static void SetReadMeContent(this BsonDocument item, string? readmeContent)
    {
        if (string.IsNullOrWhiteSpace(readmeContent))
            return;
        item.Add("readme_content", readmeContent);
    }
}
