using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RepoRanger;

public interface ISearchService
{
    Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync();
    Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null);
    Task<string?> ExtractReadmeAsync(string? githubFullName, string? branch = null);
}

public class GitHubService : ISearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly IConfiguration _configuration;

    public GitHubService(IConfiguration configuration)
    {
        _configuration = configuration;

        _apiToken = configuration["GitHubSettings:ApiToken"] ??
            throw new Exception("Error: GitHub API token not properly configured in appsettings.json");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RepoRanger", "1.0"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
    }

    public async Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync() 
    {
        var keywords = _configuration["GitHubSettings:Keywords"] ?? "fluentcms";
        var minStars = int.Parse(_configuration["GitHubSettings:MinStars"] ?? "1");
        var showForked = bool.Parse(_configuration["GitHubSettings:ShowForked"] ?? "false");
        var createdFrom = DateTime.Parse(_configuration["GitHubSettings:CreatedFrom"] ?? DateTime.UtcNow.AddYears(-1).ToString());
        var createdTo = DateTime.Parse(_configuration["GitHubSettings:CreatedTo"] ?? DateTime.UtcNow.ToString());
        var language = _configuration["GitHubSettings:Language"];

        var result = await SearchRepositoriesAsync(keywords, minStars, showForked, createdFrom, createdTo, language);

        // remove duplicate repositories by id
        return result.GroupBy(x => x.Id).Select(x => x.First());
    }

    public async Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null)
    {
        var results = new List<GitHubRepository>();

        var totalCount = await GetRepositoryCount(keywords, minStars, showForked, createdFrom, createdTo, language);

        if (totalCount == 0)
            return [];

        if (totalCount > 1000)
        {
            // calculate the number of days between the two dates
            var days = (createdTo - createdFrom).TotalDays;

            // search repositories recusrsive count with a smaller date range (1/2th of the original range)
            var halfDays = days / 2;
            var halfCreatedTo = createdFrom.AddDays(halfDays);

            Console.WriteLine($"Total repositories found: {totalCount}. Splitting search into two date ranges...");
            var firstHalfResult = await SearchRepositoriesAsync(keywords, minStars, showForked, createdFrom, halfCreatedTo, language);
            var secondHalfResult = await SearchRepositoriesAsync(keywords, minStars, showForked, halfCreatedTo, createdTo, language);

            results.AddRange(firstHalfResult);
            results.AddRange(secondHalfResult);

        }
        else
        {
            Console.WriteLine($"Total repositories found: {totalCount}. Fetching repositories...");
            var finalResults = await RetrieveRepositoriesAsync(keywords, minStars, showForked, createdFrom, createdTo, language);
            results.AddRange(finalResults);
        }
        return results;
    }

    private async Task<int> GetRepositoryCount(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null)
    {
        var searchQuery = CreateSearchQuery(keywords, createdFrom, createdTo, minStars, showForked, language, 1, 1);

        var document = await CallApi(searchQuery);

        if (document == null)
            return 0;

        var root = document.RootElement;
        if (root.TryGetProperty("total_count", out var totalCountElement))
        {
            // Get the total count of repositories
            return totalCountElement.GetInt32();
        }
        return 0;
    }

    private async Task<JsonDocument?> CallApi(string searchQuery)
    {
        var apiUrl = $"https://api.github.com/search/repositories?{searchQuery}";

        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            // Check for rate limit headers
            if (RateLimitExceeded(response, out TimeSpan waitTime))
            {
                Console.WriteLine($"Rate limit exceeded. Waiting for {waitTime.TotalMinutes:F1} minutes...");
                await Task.Delay(waitTime);
            }
            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseContent);
        }
        return null;
    }

    private static string CreateSearchQuery(string keywords, DateTime createdFrom, DateTime createdTo, int minStars, bool showForked, string? language, int perPage, int page)
    {
        var searchTerms = new List<string>
        {
            Uri.EscapeDataString(keywords),
            "archived:false",
            "is:public",
            "in:name,description,readme,topics",
            $"created:{createdFrom:yyyy-MM-dd}..{createdTo:yyyy-MM-dd}"
        };

        if (minStars > 0)
            searchTerms.Add($"stars:>={minStars}");

        if (!showForked)
            searchTerms.Add("fork:false");

        if (!string.IsNullOrWhiteSpace(language))
            searchTerms.Add($"language:{language}");

        return $"q={string.Join("+", searchTerms)}&sort=star&order=desc&per_page={perPage}&page={page}";
    }

    private async Task<IEnumerable<GitHubRepository>> RetrieveRepositoriesAsync(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null)
    {
        var results = new List<GitHubRepository>();
        int page = 1;
        int perPage = 100; // Max allowed by GitHub API
        bool hasMoreResults = true;

        // Build search query
        var searchTerms = CreateSearchQuery(keywords, createdFrom, createdTo, minStars, showForked, language, perPage, page);

        while (hasMoreResults)
        {
            Console.WriteLine($"Fetching page {page} of repositories...");
            var document = await CallApi(searchTerms);

            if (document != null)
            {
                var root = document.RootElement;

                if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    var newItems = items.EnumerateArray().ToList();
                    int itemCount = newItems.Count;

                    if (itemCount > 0)
                    {
                        // Get total count for progress reporting
                        int totalCount = 0;
                        if (root.TryGetProperty("total_count", out var totalCountElement))
                        {
                            totalCount = totalCountElement.GetInt32();
                        }

                        // Clone items to add to results
                        foreach (var item in newItems)
                            results.Add(item.ConvertToGitHubRepository());

                        // Check if we need to fetch more pages
                        if (results.Count < totalCount && itemCount == perPage)
                        {
                            page++;
                            //// GitHub API rate limiting - small delay between requests
                            //await Task.Delay(1000);
                        }
                        else
                        {
                            hasMoreResults = false;
                        }
                    }
                    else
                    {
                        hasMoreResults = false;
                    }
                }
                else
                {
                    hasMoreResults = false;
                }
            }
        }

        return results;
    }

    private static bool RateLimitExceeded(HttpResponseMessage response, out TimeSpan waitTime)
    {
        waitTime = TimeSpan.Zero;

        if (response.Headers.Contains("X-RateLimit-Remaining"))
        {
            var rateRemaining = response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault();
            if (int.TryParse(rateRemaining, out int remaining) && remaining < 5)
            {
                // If rate limit is about to be reached, calculate wait time until reset
                if (response.Headers.Contains("X-RateLimit-Reset"))
                {
                    var resetTimeStr = response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault();
                    if (long.TryParse(resetTimeStr, out long resetTime))
                    {
                        var resetDateTime = DateTimeOffset.FromUnixTimeSeconds(resetTime).DateTime;
                        waitTime = resetDateTime - DateTime.UtcNow;
                        return waitTime.TotalSeconds > 0;
                    }
                }
                return true;
            }
        }
        return false;
    }

    public async Task<string?> ExtractReadmeAsync(string? githubFullName, string? branch = null)
    {
        Console.WriteLine($"Extracting README for {githubFullName}...");
        if (string.IsNullOrWhiteSpace(githubFullName))
            return null;

        // First, get the default branch if not specified
        if (string.IsNullOrWhiteSpace(branch))
            return null;

        try
        {
            // Construct the README API URL
            string apiUrl = $"https://api.github.com/repos/{githubFullName}/readme";

            // Add branch parameter if specified
            if (!string.IsNullOrWhiteSpace(branch))
                apiUrl += $"?ref={Uri.EscapeDataString(branch)}";

            // Send request to GitHub API
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            // Check if README exists
            if (!response.IsSuccessStatusCode)
                return null;

            if (response.IsSuccessStatusCode)
            {
                // Check for rate limit headers
                if (RateLimitExceeded(response, out TimeSpan waitTime))
                {
                    Console.WriteLine($"Rate limit exceeded. Waiting for {waitTime.TotalMinutes:F1} minutes...");
                    await Task.Delay(waitTime);
                }
            }

            // Parse the response
            string responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            // Check if content exists
            if (!root.TryGetProperty("content", out var contentElement) || !root.TryGetProperty("encoding", out var encodingElement))
                return null;

            var encodingType = encodingElement.GetString() ??
                throw new Exception("Could not determine encoding type");

            var encodedContent = contentElement.GetString() ??
                throw new Exception("Could not determine encoded content");

            // Decode the content based on encoding
            string readmeContent = DecodeReadmeContent(encodedContent, encodingType);

            return readmeContent;
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error extracting README: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Decodes the README content based on the encoding type
    /// </summary>
    private static string DecodeReadmeContent(string content, string encodingType)
    {
        switch (encodingType)
        {
            case "base64":
                // Remove any whitespace from the base64 string
                content = content.Replace("\n", "").Replace("\r", "");

                // Decode base64
                byte[] data = Convert.FromBase64String(content);
                return Encoding.UTF8.GetString(data);

            default:
                throw new NotSupportedException($"Unsupported encoding type: {encodingType}");
        }
    }

}
