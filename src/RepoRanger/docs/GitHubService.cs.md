# GitHubService.cs

## Overview

This file defines the GitHub API communication service for the RepoRanger application. It contains:

1. The `ISearchService` interface that defines the contract for repository search operations
2. The `GitHubService` class that implements this interface to interact with the GitHub API

The service provides functionality to:
- Search for GitHub repositories based on various criteria
- Extract README content from repositories
- Handle GitHub API rate limiting and pagination

## Components

### ISearchService Interface

```csharp
public interface ISearchService
{
    Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync();
    Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null);
    Task<string?> ExtractReadmeAsync(string? githubFullName, string? branch = null);
}
```

This interface defines the contract for repository search and content extraction:

| Method | Parameters | Description |
|--------|------------|-------------|
| `SearchRepositoriesAsync()` | None | Searches for repositories using settings from the configuration |
| `SearchRepositoriesAsync(...)` | Various search criteria | Searches for repositories matching the specified criteria |
| `ExtractReadmeAsync(...)` | Repository identifiers | Extracts README content for a specific repository |

### GitHubService Class

This class implements the `ISearchService` interface to interact with the GitHub API.

#### Constructor

```csharp
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
```

The constructor:
1. Stores the provided configuration
2. Retrieves the GitHub API token from the configuration (throwing an exception if not found)
3. Initializes an HTTP client with appropriate headers for GitHub API communication

#### Primary Methods

##### SearchRepositoriesAsync() (Parameterless)

This method retrieves search criteria from the application configuration and delegates to the parameterized version of the method:

1. Reads search parameters from configuration (with defaults if not specified)
2. Calls the parameterized `SearchRepositoriesAsync` method
3. Removes duplicate repositories by ID

##### SearchRepositoriesAsync(...) (Parameterized)

This is the core repository search method that:

1. Checks the total count of matching repositories
2. If more than 1000 repositories match (GitHub API limit), recursively splits the date range to fetch all results
3. Otherwise, retrieves the repositories using pagination
4. Handles potential rate limiting issues

This method implements a sophisticated date-based pagination strategy to work around GitHub's API limits:

```csharp
if (totalCount > 1000)
{
    // calculate the number of days between the two dates
    var days = (createdTo - createdFrom).TotalDays;

    // search repositories recursively with a smaller date range (1/2th of the original range)
    var halfDays = days / 2;
    var halfCreatedTo = createdFrom.AddDays(halfDays);

    Console.WriteLine($"Total repositories found: {totalCount}. Splitting search into two date ranges...");
    var firstHalfResult = await SearchRepositoriesAsync(keywords, minStars, showForked, createdFrom, halfCreatedTo, language);
    var secondHalfResult = await SearchRepositoriesAsync(keywords, minStars, showForked, halfCreatedTo, createdTo, language);

    results.AddRange(firstHalfResult);
    results.AddRange(secondHalfResult);
}
```

##### ExtractReadmeAsync

This method fetches and decodes README content for a specific repository:

1. Constructs the appropriate GitHub API URL for README content
2. Sends a request with proper authentication
3. Handles rate limiting if necessary
4. Extracts the base64-encoded content from the response
5. Decodes the content to a string

#### Helper Methods

##### GetRepositoryCount

Determines the total number of repositories matching a search query:

```csharp
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
```

##### CallApi

Sends a request to the GitHub API and handles common response scenarios:

```csharp
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
```

##### CreateSearchQuery

Builds a GitHub search query URL with specified parameters:

```csharp
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
```

##### RetrieveRepositoriesAsync

Fetches repository data from GitHub using pagination:

```csharp
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

        // Handle response and process results
        // ...
    }

    return results;
}
```

##### RateLimitExceeded

Checks if the GitHub API rate limit has been exceeded and calculates the wait time:

```csharp
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
```

##### DecodeReadmeContent

Decodes the README content based on the encoding type:

```csharp
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
```

## Dependencies

- System.Net.Http.Headers (for HTTP request headers)
- System.Text (for encoding operations)
- System.Text.Json (for JSON parsing)
- Microsoft.Extensions.Configuration (for accessing configuration)
- RepoRanger.GitHubRepository (model class for GitHub repositories)

## Configuration Requirements

The following settings should be defined in the appsettings.json file:

```json
{
  "GitHubSettings": {
    "ApiToken": "your-github-api-token",
    "Keywords": "search-keywords",
    "MinStars": 1,
    "ShowForked": false,
    "CreatedFrom": "2023-01-01",
    "CreatedTo": "2024-01-01",
    "Language": "csharp"
  }
}
```

All settings except `ApiToken` have default values if not specified.
