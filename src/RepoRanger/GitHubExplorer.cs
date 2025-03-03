//using Microsoft.Extensions.Configuration;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using RepoRanger.Models;
//using System.Net.Http.Headers;
//using System.Text.Json;

//namespace RepoRanger;

//public class GitHubExplorer
//{
//    private readonly string _gitHubSearchApiUrl = "https://api.github.com/search/repositories";
//    private readonly string _apiToken;

//    private static readonly int PerPage = 100; // Maximum allowed per request
//    private static readonly int MaxPages = 10; // GitHub allows max 1000 results (100 per page * 10 pages)
//    private static readonly int MinStars = 50; // Minimum stars to filter
//    private static readonly string PushedDateFilter = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");

//    public GitHubExplorer(IConfiguration configuration)
//    {
//        // Get GitHub API token from configuration
//        _apiToken = configuration["GitHubSettings:ApiToken"] ??
//            throw new Exception("Error: GitHub API token not properly configured in appsettings.json");
//    }

//    public async Task<List<GitHubRepository>> SearchRepositoriesAsync(List<string> keywords)
//    {
//        var query = string.Join("+OR+", keywords);
//        var page = 1;
//        var allRepositories = new List<GitHubRepository>();

//        while (page <= MaxPages)
//        {
//            var searchQuery = $"q={query}+stars:>{MinStars}+pushed:>={PushedDateFilter}&sort=updated&order=desc&per_page={PerPage}&page={page}";
//            var repositories = await FetchRepositoriesAsync(searchQuery);

//            if (repositories.Count == 0) break; // Stop if no more results

//            allRepositories.AddRange(repositories);
//            page++;
//        }

//        return allRepositories;
//    }

//    private async Task<JsonDocument> FetchRepositoriesAsync(string query, int page)
//    {
//        using var client = new HttpClient();

//        // Set up GitHub API request headers
//        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
//        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
//        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

//        var response = await client.GetAsync($"{_gitHubSearchApiUrl}?{query}");
//        if (response.IsSuccessStatusCode)
//        {
//            string responseContent = await response.Content.ReadAsStringAsync();
//            var searchResultJson = JsonDocument.Parse(responseContent);

//            // Get the items array from the search result
//            if (searchResultJson.RootElement.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
//            {
//                var items = itemsElement.EnumerateArray().ToList();
//                int itemCount = items.Count;

//                if (itemCount > 0)
//                {
//                    // Get total count for progress reporting
//                    int totalCount = 0;
//                    if (searchResultJson.RootElement.TryGetProperty("total_count", out var totalCountElement))
//                    {
//                        totalCount = totalCountElement.GetInt32();
//                    }
//                }
//                {
//                    Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
//                    return [];
//                }

//                var jsonResponse = await response.Content.ReadAsStringAsync();
//                var result = JsonSerializer.Deserialize<GitHubSearchResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//                return result?.Items ?? [];
//            }

//        }
//    }
//}