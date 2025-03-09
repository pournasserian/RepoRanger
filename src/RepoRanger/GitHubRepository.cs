namespace RepoRanger;

public class GitHubRepository
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? FullName { get; set; }
    public string? HtmlUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime PushedAt { get; set; }
    public string? Language { get; set; }
    public string? HomePage { get; set; }
    public long Size { get; set; }
    public int StargazersCount { get; set; }
    public int WatchersCount { get; set; }
    public int ForksCount { get; set; }
    public int OpenIssuesCount { get; set; }
    public List<string?> Topics { get; set; } = [];
    public string? DefaultBranch { get; set; }
    public bool IsTemplate { get; set; }
    public string? ReadMeContent { get; set; }
    public DateTime ReadDate { get; set; } = DateTime.UtcNow;
}
