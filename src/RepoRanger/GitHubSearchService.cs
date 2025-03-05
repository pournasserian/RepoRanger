using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RepoRanger;

public interface ISearchService
{
    Task<IEnumerable<JsonElement>> SearchRepositoriesAsync(string keywords, int minStars = 0, bool showForked = false, int minLastActivityDays = 0, string? language = null, int maxResults = 0);
}

public class GitHubSearchService : ISearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;

    public GitHubSearchService(IConfiguration configuration)
    {
        _apiToken = configuration["GitHubSettings:ApiToken"] ??
            throw new Exception("Error: GitHub API token not properly configured in appsettings.json");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RepoRanger", "1.0"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
    }

    public async Task<IEnumerable<JsonElement>> SearchRepositoriesAsync(string keywords, int minStars = 0, bool showForked = false, int minLastActivityDays = 0, string? language = null, int maxResults = 0)
    {
        var results = new List<JsonElement>();
        var monthSteps = 1;
        var yearsToSearch = 1;
        var createdFrom = DateTime.UtcNow.AddYears(-yearsToSearch);
        var createdTo = createdFrom.AddMonths(monthSteps);

        // loop over all months in the last 5 years
        while (createdFrom < DateTime.UtcNow)
        {
            var searchResults = await SearchRepositoriesAsync(keywords, createdFrom, createdTo, minStars, showForked, minLastActivityDays, language, maxResults);
            results.AddRange(searchResults);
            createdFrom = createdTo;
            createdTo = createdTo.AddMonths(monthSteps);
        }

        return results;
    }

    private async Task<IEnumerable<JsonElement>> SearchRepositoriesAsync(string keywords, DateTime createdFrom, DateTime createdTo, int minStars = 0, bool showForked = false, int minLastActivityDays = 0, string? language = null, int maxResults = 0)
    {
        if (string.IsNullOrWhiteSpace(keywords))
            throw new ArgumentException("Keywords cannot be empty", nameof(keywords));

        var results = new List<JsonElement>();
        int page = 1;
        int perPage = 100; // Max allowed by GitHub API
        bool hasMoreResults = true;
        var activityDate = DateTime.UtcNow.AddDays(-minLastActivityDays).ToString("yyyy-MM-dd");

        // Build search query
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

        while (hasMoreResults)
        {
            var searchQuery = $"q={string.Join("+", searchTerms)}&sort=star&order=desc&per_page={perPage}&page={page}";

            Console.WriteLine($"Searching GitHub for: {searchQuery}");

            string apiUrl = $"https://api.github.com/search/repositories?{searchQuery}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;

                if (root.TryGetProperty("items", out var items) &&
                    items.ValueKind == JsonValueKind.Array)
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
                        {
                            results.Add(JsonDocument.Parse(item.GetRawText()).RootElement.Clone());
                        }

                        Console.WriteLine($"Retrieved page {page} - Found {itemCount} repositories (Total so far: {results.Count}/{totalCount})");

                        // Check if we need to fetch more pages
                        if ((maxResults == 0 || results.Count < maxResults) &&
                            results.Count < totalCount &&
                            itemCount == perPage)
                        {
                            page++;

                            // Check if we've hit our max results target
                            if (maxResults > 0 && results.Count + perPage > maxResults)
                            {
                                // Adjust perPage for the last request to get exactly maxResults
                                perPage = maxResults - results.Count;
                            }

                            // GitHub API rate limiting - small delay between requests
                            await Task.Delay(1000);
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
                    Console.WriteLine("No items found in search results");
                    hasMoreResults = false;
                }

                // Check for rate limit headers
                if (RateLimitExceeded(response, out TimeSpan waitTime))
                {
                    Console.WriteLine($"Rate limit exceeded. Waiting for {waitTime.TotalMinutes:F1} minutes...");
                    await Task.Delay(waitTime);
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                hasMoreResults = false;
            }

            // Limit results if maxResults specified
            if (maxResults > 0 && results.Count >= maxResults)
            {
                results = results.Take(maxResults).ToList();
                hasMoreResults = false;
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

}
