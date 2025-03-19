namespace RepoRanger;

/// <summary>
/// Represents a GitHub repository with its metadata and README content.
/// This model class maps to the GitHub API response structure with additional
/// fields for storing extracted README content.
/// </summary>
public class GitHubRepository
{
    // Unique identifier for the repository
    public long Id { get; set; }

    // Repository name without owner
    public string? Name { get; set; }

    // Full repository name including owner (e.g., "owner/repo")
    // Used for making further API calls to the repository
    public string? FullName { get; set; }

    // URL to the repository's GitHub page
    public string? HtmlUrl { get; set; }

    // Repository description text
    public string? Description { get; set; }

    // Repository creation timestamp
    public DateTime CreatedAt { get; set; }

    // Last update timestamp
    public DateTime UpdatedAt { get; set; }

    // Last push timestamp
    public DateTime PushedAt { get; set; }

    // Primary programming language used in the repository
    public string? Language { get; set; }

    // External homepage URL if specified
    public string? HomePage { get; set; }

    // Repository size in KB
    public long Size { get; set; }

    // Number of stars the repository has received
    public int StargazersCount { get; set; }

    // Number of users watching the repository
    public int WatchersCount { get; set; }

    // Number of repository forks
    public int ForksCount { get; set; }

    // Number of open issues in the repository
    public int OpenIssuesCount { get; set; }

    // Repository topics/tags
    public List<string?> Topics { get; set; } = [];

    // Default branch name (e.g., "main" or "master")
    // Used for extracting README content
    public string? DefaultBranch { get; set; }

    // Indicates if this repository is a template repository
    public bool IsTemplate { get; set; }

    // Extracted README content
    // This field is populated after fetching the README from GitHub
    public string? ReadMeContent { get; set; }

    // Timestamp when the README was last fetched
    public DateTime ReadDate { get; set; } = DateTime.UtcNow;
}
