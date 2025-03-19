# GitHubRepository.cs

## Overview

This file defines the `GitHubRepository` model class that represents a GitHub repository entity in the RepoRanger application. It serves as a data container for GitHub repository information retrieved via the GitHub API and stored in MongoDB.

## Class Definition

```csharp
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
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `long` | The unique identifier of the repository assigned by GitHub |
| `Name` | `string?` | The name of the repository |
| `FullName` | `string?` | The full name of the repository (owner/name) |
| `HtmlUrl` | `string?` | The URL to view the repository on GitHub |
| `Description` | `string?` | The description of the repository |
| `CreatedAt` | `DateTime` | The date and time when the repository was created |
| `UpdatedAt` | `DateTime` | The date and time when the repository was last updated |
| `PushedAt` | `DateTime` | The date and time when the repository was last pushed to |
| `Language` | `string?` | The primary programming language of the repository |
| `HomePage` | `string?` | The homepage URL of the repository |
| `Size` | `long` | The size of the repository in kilobytes |
| `StargazersCount` | `int` | The number of users who have starred the repository |
| `WatchersCount` | `int` | The number of users watching the repository |
| `ForksCount` | `int` | The number of times the repository has been forked |
| `OpenIssuesCount` | `int` | The number of open issues in the repository |
| `Topics` | `List<string?>` | A list of topics associated with the repository |
| `DefaultBranch` | `string?` | The default branch of the repository (e.g., 'main' or 'master') |
| `IsTemplate` | `bool` | Whether the repository is a template repository |
| `ReadMeContent` | `string?` | The content of the repository's README file |
| `ReadDate` | `DateTime` | The date and time when the README content was last read (defaults to UTC now) |

## Notes

- The class uses nullable reference types (`string?`) for properties that might not have values, in accordance with .NET's nullable reference type feature.
- The `Topics` property is initialized with an empty collection initializer (`= []`), a feature added in C# 12.
- The `ReadDate` property is automatically initialized to the current UTC time when an instance is created.
- This model is designed to be compatible with both the GitHub API response structure and MongoDB storage.

## Usage

The `GitHubRepository` class is used in several places throughout the application:

1. In the `GitHubService` class, JSON responses from the GitHub API are converted to `GitHubRepository` objects.
2. In the `MongoDbRepository` class, `GitHubRepository` objects are stored in and retrieved from MongoDB.
3. In the `Program.cs` file, collections of `GitHubRepository` objects are processed to extract README content.
