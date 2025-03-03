using MongoDB.Bson.Serialization.Attributes;

namespace RepoRanger.Models;

public class GitHubOwner
{
    [BsonElement("login")]
    public string Login { get; set; } = default!;

    [BsonElement("id")]
    public long Id { get; set; }

    [BsonElement("html_url")]
    public string HtmlUrl { get; set; } = default!;
}