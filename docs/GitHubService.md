---
layout: page
title: GitHubService
---

# GitHubService

## Overview
The `GitHubService` class is a crucial component of the RepoRanger application that implements search functionality for GitHub repositories via the GitHub API. It allows developers to execute various repository search queries and extract README content, facilitating interaction with GitHub data and its seamless integration into other application modules.

## Key Features
- **SearchRepositoriesAsync**: Asynchronously searches for repositories based on specified criteria such as keywords, minimum stars, and created date range, allowing for efficient retrieval of relevant repositories.
- **ExtractReadmeAsync**: Fetches and decodes the README content of a specified repository, providing a convenient way to obtain documentation directly from GitHub.
- **Configuration-Driven**: Utilizes configuration settings (e.g., API token, GitHub search filters) defined in the `appsettings.json` file, enabling flexibility and ease of modifications.

## Class Structure
### Properties
- **HttpClient _httpClient**: The HttpClient used for making API calls to GitHub.
- **string _apiToken**: The token used for authentication to access the GitHub API, retrieved from configuration settings.
- **IConfiguration _configuration**: An instance of IConfiguration that provides access to application settings.

### Methods
#### public async Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync()
- Searches repositories using default settings defined in configuration.
- Returns a unique list of GitHub repositories based on the search criteria.

#### public async Task<IEnumerable<GitHubRepository>> SearchRepositoriesAsync(string keywords, int minStars, bool showForked, DateTime createdFrom, DateTime createdTo, string? language = null)
- Executes a search for repositories with specified parameters.
- Offers flexibility in refining search results by various criteria.

#### public async Task<string?> ExtractReadmeAsync(string? githubFullName, string? branch = null)
- Retrieves the README content for a given repository, specified by its full name and optionally a branch.
- Handles content decoding based on encoding type, ensuring properly formatted output.

## Example Usage
```csharp
var githubService = new GitHubService(configuration);
var repositories = await githubService.SearchRepositoriesAsync("fluentcms", 10, false, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
var readmeContent = await githubService.ExtractReadmeAsync("owner/repo", "main");
```

## Conclusion
The `GitHubService` class is an essential part of the RepoRanger application, providing robust functionality for searching GitHub repositories and extracting their corresponding README files. By leveraging the GitHub API, it enables developers to build applications that interact effectively with GitHub data.
