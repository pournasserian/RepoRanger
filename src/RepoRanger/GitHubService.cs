using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RepoRanger;

/// <summary>
/// Interface for repository search and content extraction services.
/// Defines methods for searching repositories and extracting README content.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Search repositories using configuration settings from appsettings.json.
    /// </summary>
    Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync();

    /// <summary>
    /// Search repositories with explicit parameters.
    /// </summary>
    Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync(
        string keywords,
        int minStars,
        bool showForked,
        DateTime createdFrom,
        DateTime createdTo,
        string? language = null);

    /// <summary>
    /// Extract README content from a GitHub repository.
    /// </summary>
    Task<string?> ExtractReadmeAsync(string? githubFullName, string? branch = null);
}

/// <summary>
/// GitHub API implementation of the ISearchService interface.
/// Handles searching repositories and extracting README content from GitHub.
/// </summary>
public class GitHubService : ISearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initialize the GitHub service with configuration settings.
    /// </summary>
    /// <param name="configuration">Application configuration with GitHub settings</param>
    public GitHubService(IConfiguration configuration)
    {
        _configuration = configuration;

        // Get API token from configuration
        // GitHub requires an API token for authenticated requests
        _apiToken = configuration["GitHubSettings:ApiToken"] ??
            throw new Exception("Error: GitHub API token not properly configured in appsettings.json");

        // Set up HTTP client with appropriate headers for GitHub API
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RepoRanger", "1.0"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
    }

    /// <summary>
    /// Search repositories using settings from configuration.
    /// </summary>
    /// <returns>Collection of GitHub repositories matching the criteria</returns>
    public async Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync()
    {
        // Read search parameters from configuration
        // Default values are provided as fallbacks if configuration is missing
        var keywords = _configuration["GitHubSettings:Keywords"] ?? "fluentcms";
        var minStars = int.Parse(_configuration["GitHubSettings:MinStars"] ?? "1");
        var showForked = bool.Parse(_configuration["GitHubSettings:ShowForked"] ?? "false");
        var createdFrom = DateTime.Parse(_configuration["GitHubSettings:CreatedFrom"] ?? DateTime.UtcNow.AddYears(-1).ToString());
        var createdTo = DateTime.Parse(_configuration["GitHubSettings:CreatedTo"] ?? DateTime.UtcNow.ToString());
        var language = _configuration["GitHubSettings:Language"];

        // Delegate to the parameterized search method
        var result = await SearchRepositoriesAsync(keywords, minStars, showForked, createdFrom, createdTo, language);

        // Remove duplicate repositories by id
        // This ensures we don't process or store the same repository multiple times
        return result.GroupBy(x => x.Id).Select(x => x.First());
    }

    /// <summary>
    /// Search GitHub repositories with explicit search parameters.
    /// </summary>
    /// <param name="keywords">Search terms</param>
    /// <param name="minStars">Minimum stars filter</param>
    /// <param name="showForked">Whether to include forked repositories</param>
    /// <param name="createdFrom">Start date for repository creation</param>
    /// <param name="createdTo">End date for repository creation</param>
    /// <param name="language">Programming language filter</param>
    /// <returns>Collection of GitHub repositories matching the criteria</returns>
    public async Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null)
    {
        var results = new List<GitHubRepository>();

        // First, get the total count of repositories matching the search criteria
        var totalCount = await GetRepositoryCount(keywords, minStars, showForked, createdFrom, createdTo, language);

        if (totalCount == 0)
            return []; // Return empty collection if no results

        // GitHub API has a limit of 1000 results (even with pagination)
        // If more than 1000 results, we need to split the search into smaller chunks
        if (totalCount > 1000)
        {
            // Calculate the number of days between the two dates
            var days = (createdTo - createdFrom).TotalDays;

            // Split the date range in half to reduce result set size
            var halfDays = days / 2;
            var halfCreatedTo = createdFrom.AddDays(halfDays);

            Console.WriteLine($"Total repositories found: {totalCount}. Splitting search into two date ranges...");

            // Recursively search each half of the date range
            // This divide-and-conquer approach ensures we can get all results
            var firstHalfResult = await SearchRepositoriesAsync(keywords, minStars, showForked, createdFrom, halfCreatedTo, language);
            var secondHalfResult = await SearchRepositoriesAsync(keywords, minStars, showForked, halfCreatedTo, createdTo, language);

            // Combine the results from both halves
            results.AddRange(firstHalfResult);
            results.AddRange(secondHalfResult);
        }
        else
        {
            // If under 1000 results, we can fetch them directly with pagination
            Console.WriteLine($"Total repositories found: {totalCount}. Fetching repositories...");
            var finalResults = await RetrieveRepositoriesAsync(keywords, minStars, showForked, createdFrom, createdTo, language);
            results.AddRange(finalResults);
        }
        return results;
    }

    /// <summary>
    /// Get the total count of repositories matching search criteria.
    /// </summary>
    /// <returns>Total number of matching repositories</returns>
    /// <remarks>
    /// This is used to determine whether we need to split the search or
    /// if we can fetch all results directly.
    /// </remarks>
    private async Task<int> GetRepositoryCount(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null)
    {
        // Create a search query with minimal results (page size 1)
        // We only need the total count, not the actual repositories
        var searchQuery = CreateSearchQuery(keywords, createdFrom, createdTo, minStars, showForked, language, 1, 1);

        // Make API call to get search results
        var document = await CallApi(searchQuery);

        if (document == null)
            return 0;

        // Parse the response to get the total count
        var root = document.RootElement;
        if (root.TryGetProperty("total_count", out var totalCountElement))
        {
            // Get the total count of repositories
            return totalCountElement.GetInt32();
        }
        return 0;
    }

    /// <summary>
    /// Make a request to the GitHub API.
    /// </summary>
    /// <param name="searchQuery">Query string for the API request</param>
    /// <returns>JSON document from API response, or null if request failed</returns>
    private async Task<JsonDocument?> CallApi(string searchQuery)
    {
        // Construct the full API URL with the search query
        var apiUrl = $"https://api.github.com/search/repositories?{searchQuery}";

        // Send the request to the GitHub API
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            // Check if we're approaching GitHub's rate limit
            // If so, pause execution until the rate limit resets
            if (RateLimitExceeded(response, out TimeSpan waitTime))
            {
                Console.WriteLine($"Rate limit exceeded. Waiting for {waitTime.TotalMinutes:F1} minutes...");
                await Task.Delay(waitTime);
            }

            // Parse the JSON response
            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseContent);
        }
        return null;
    }

    /// <summary>
    /// Create a GitHub search query string from search parameters.
    /// </summary>
    /// <returns>Properly formatted query string for GitHub API</returns>
    private static string CreateSearchQuery(string keywords, DateTime createdFrom, DateTime createdTo, int minStars, bool showForked, string? language, int perPage, int page)
    {
        // Build list of search qualifiers based on parameters
        var searchTerms = new List<string>
        {
            // Main search keywords
            Uri.EscapeDataString(keywords),
            
            // Only include non-archived repositories
            "archived:false",
            
            // Only include public repositories
            "is:public",
            
            // Search in repository name, description, readme, and topics
            "in:name,description,readme,topics",
            
            // Date range filter for repository creation date
            $"created:{createdFrom:yyyy-MM-dd}..{createdTo:yyyy-MM-dd}"
        };

        // Add minimum stars filter if specified
        if (minStars > 0)
            searchTerms.Add($"stars:>={minStars}");

        // Filter out forks if not requested
        if (!showForked)
            searchTerms.Add("fork:false");

        // Add language filter if specified
        if (!string.IsNullOrWhiteSpace(language))
            searchTerms.Add($"language:{language}");

        // Combine all terms and add sorting and pagination parameters
        return $"q={string.Join("+", searchTerms)}&sort=star&order=desc&per_page={perPage}&page={page}";
    }

    /// <summary>
    /// Retrieve repositories by fetching multiple pages of results.
    /// </summary>
    /// <returns>Collection of GitHub repositories</returns>
    private async Task<IEnumerable<GitHubRepository>> RetrieveRepositoriesAsync(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null)
    {
        var results = new List<GitHubRepository>();
        int page = 1;
        int perPage = 100; // Maximum page size allowed by GitHub API
        bool hasMoreResults = true;

        // Build initial search query for the first page
        var searchQuery = CreateSearchQuery(keywords, createdFrom, createdTo, minStars, showForked, language, perPage, page);

        // Continue fetching pages until we have all results or reach GitHub's limit
        while (hasMoreResults)
        {
            Console.WriteLine($"Fetching page {page} of repositories...");
            var document = await CallApi(searchQuery);

            if (document != null)
            {
                var root = document.RootElement;

                // Process the items array from the response
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

                        // Convert each item to a GitHubRepository object and add to results
                        foreach (var item in newItems)
                            results.Add(item.ConvertToGitHubRepository());

                        // Check if we need to fetch more pages
                        if (results.Count < totalCount && itemCount == perPage)
                        {
                            // Move to the next page
                            page++;
                            // Update the search query for the next page
                            searchQuery = CreateSearchQuery(keywords, createdFrom, createdTo, minStars, showForked, language, perPage, page);

                            // Uncomment this to add delay between requests if needed
                            // await Task.Delay(1000);
                        }
                        else
                        {
                            // No more pages needed
                            hasMoreResults = false;
                        }
                    }
                    else
                    {
                        // Empty page, no more results
                        hasMoreResults = false;
                    }
                }
                else
                {
                    // No items property in response
                    hasMoreResults = false;
                }
            }
            else
            {
                // API call failed
                hasMoreResults = false;
            }
        }

        return results;
    }

    /// <summary>
    /// Check if GitHub API rate limit is about to be exceeded.
    /// </summary>
    /// <param name="response">HTTP response from GitHub API</param>
    /// <param name="waitTime">Time to wait until rate limit resets</param>
    /// <returns>True if rate limit is approaching, false otherwise</returns>
    private static bool RateLimitExceeded(HttpResponseMessage response, out TimeSpan waitTime)
    {
        waitTime = TimeSpan.Zero;

        // GitHub includes rate limit information in response headers
        if (response.Headers.Contains("X-RateLimit-Remaining"))
        {
            // Check how many API calls we have left
            var rateRemaining = response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault();
            if (int.TryParse(rateRemaining, out int remaining) && remaining < 5)
            {
                // If we're about to hit the rate limit (less than 5 calls left),
                // calculate how long to wait until the limit resets
                if (response.Headers.Contains("X-RateLimit-Reset"))
                {
                    // Get the Unix timestamp when the rate limit resets
                    var resetTimeStr = response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault();
                    if (long.TryParse(resetTimeStr, out long resetTime))
                    {
                        // Convert Unix timestamp to DateTime
                        var resetDateTime = DateTimeOffset.FromUnixTimeSeconds(resetTime).DateTime;

                        // Calculate wait time until reset
                        waitTime = resetDateTime - DateTime.UtcNow;

                        // Only return true if we actually need to wait
                        return waitTime.TotalSeconds > 0;
                    }
                }
                // If we can't determine wait time but are near limit, return true anyway
                return true;
            }
        }
        // Rate limit is not about to be exceeded
        return false;
    }

    /// <summary>
    /// Extract README content from a GitHub repository.
    /// </summary>
    /// <param name="githubFullName">Full repository name (owner/repo)</param>
    /// <param name="branch">Branch to get README from</param>
    /// <returns>Decoded README content, or null if not found</returns>
    public async Task<string?> ExtractReadmeAsync(string? githubFullName, string? branch = null)
    {
        Console.WriteLine($"Extracting README for {githubFullName}...");

        // Validate parameters
        if (string.IsNullOrWhiteSpace(githubFullName))
            return null;

        // Require a branch to extract the README from
        if (string.IsNullOrWhiteSpace(branch))
            return null;

        try
        {
            // Construct the GitHub API URL for fetching README
            string apiUrl = $"https://api.github.com/repos/{githubFullName}/readme";

            // Add branch parameter to get README from the specific branch
            if (!string.IsNullOrWhiteSpace(branch))
                apiUrl += $"?ref={Uri.EscapeDataString(branch)}";

            // Send request to GitHub API
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            // Check if README exists
            if (!response.IsSuccessStatusCode)
                return null;

            // Handle rate limiting
            if (response.IsSuccessStatusCode)
            {
                // Check for rate limit headers
                if (RateLimitExceeded(response, out TimeSpan waitTime))
                {
                    Console.WriteLine($"Rate limit exceeded. Waiting for {waitTime.TotalMinutes:F1} minutes...");
                    await Task.Delay(waitTime);
                }
            }

            // Parse the JSON response
            string responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            // Extract content and encoding information
            // GitHub returns README content as base64-encoded
            if (!root.TryGetProperty("content", out var contentElement) || !root.TryGetProperty("encoding", out var encodingElement))
                return null;

            // Get encoding type (should be "base64")
            var encodingType = encodingElement.GetString() ??
                throw new Exception("Could not determine encoding type");

            // Get the encoded content
            var encodedContent = contentElement.GetString() ??
                throw new Exception("Could not determine encoded content");

            // Decode the content based on encoding type
            string readmeContent = DecodeReadmeContent(encodedContent, encodingType);

            return readmeContent;
        }
        catch (Exception ex)
        {
            // Log the error and return null
            Console.WriteLine($"Error extracting README: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Decodes the README content based on the encoding type.
    /// </summary>
    /// <param name="content">Encoded content</param>
    /// <param name="encodingType">Encoding type (e.g., "base64")</param>
    /// <returns>Decoded content as string</returns>
    /// <exception cref="NotSupportedException">Thrown if encoding type is not supported</exception>
    private static string DecodeReadmeContent(string content, string encodingType)
    {
        switch (encodingType)
        {
            case "base64":
                // GitHub API includes newlines in the base64 string for formatting
                // These need to be removed before decoding
                content = content.Replace("\n", "").Replace("\r", "");

                // Convert from base64 to bytes, then to UTF-8 string
                byte[] data = Convert.FromBase64String(content);
                return Encoding.UTF8.GetString(data);

            default:
                throw new NotSupportedException($"Unsupported encoding type: {encodingType}");
        }
    }

}
