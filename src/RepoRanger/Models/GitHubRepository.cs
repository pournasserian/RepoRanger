using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepoRanger.Models;

public class GitHubRepository
{
    [BsonId]
    public long Id { get; set; }

    [BsonElement("repo_id")]
    public long RepoId { get; set; }

    [BsonElement("full_name")]
    public string FullName { get; set; } = default!;

    [BsonElement("html_url")]
    public string HtmlUrl { get; set; } = default!;

    [BsonElement("description")]
    public string Description { get; set; } = default!;

    [BsonElement("language")]
    public string Language { get; set; } = default!;

    [BsonElement("stargazers_count")]
    public int StargazersCount { get; set; }

    [BsonElement("forks_count")]
    public int ForksCount { get; set; }

    [BsonElement("open_issues_count")]
    public int OpenIssuesCount { get; set; }

    [BsonElement("default_branch")]
    public string DefaultBranch { get; set; } = default!;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("pushed_at")]
    public DateTime PushedAt { get; set; }

    [BsonElement("owner")]
    public GitHubOwner Owner { get; set; } = default!;
}
