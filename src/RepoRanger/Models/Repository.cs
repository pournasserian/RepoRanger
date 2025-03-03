namespace RepoRanger.Models;

public class Repository : GitHubRepository
{
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public DateTime UpdateDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = string.Empty;
}