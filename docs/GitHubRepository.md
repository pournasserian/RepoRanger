---
layout: page
title: Developer Documentation for GitHubRepository
---

# Developer Documentation for GitHubRepository

## Overview
The `GitHubRepository` class is a key component of the RepoRanger application, representing a GitHub repository with various properties that store essential information about the repository. This class is utilized to manage data retrieved from the GitHub API and facilitate operations such as data storage and manipulations.

## Properties
- **Id**: `long` - The unique identifier for the repository.
- **Name**: `string?` - The name of the repository.
- **FullName**: `string?` - The full name of the repository, typically in the format `owner/repo`.
- **HtmlUrl**: `string?` - The URL to access the repository on GitHub.
- **Description**: `string?` - A brief description of the repository provided by the owner.
- **CreatedAt**: `DateTime` - The date and time when the repository was created.
- **UpdatedAt**: `DateTime` - The date and time when the repository was last updated.
- **PushedAt**: `DateTime` - The date and time when the repository was last pushed to.
- **Language**: `string?` - The primary programming language used in the repository.
- **HomePage**: `string?` - The homepage URL for the repository, if any.
- **Size**: `long` - The size of the repository in kilobytes.
- **StargazersCount**: `int` - The number of stars the repository has received from users.
- **WatchersCount**: `int` - The number of users watching the repository.
- **ForksCount**: `int` - The number of forks of the repository.
- **OpenIssuesCount**: `int` - The number of open issues in the repository.
- **Topics**: `List<string?>` - A list of topics associated with the repository, which helps in categorizing it.
- **DefaultBranch**: `string?` - The default branch for the repository (e.g., `main` or `master`).
- **IsTemplate**: `bool` - Indicates whether the repository is a template for creating new repositories.
- **ReadMeContent**: `string?` - The content of the README file extracted from the repository.
- **ReadDate**: `DateTime` - The date and time when the README content was last read or updated.

## Usage
The `GitHubRepository` class is mainly used within the MongoDB storage implementation to encapsulate each repository's data, allowing for efficient data retrieval, updates, and storage within the application.

## Example
```csharp
var repository = new GitHubRepository
{
    Id = 123456,
    Name = "example-repo",
    FullName = "owner/example-repo",
    HtmlUrl = "https://github.com/owner/example-repo",
    Description = "An example repository for demonstration purposes.",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    PushedAt = DateTime.UtcNow,
    Language = "C#",
    Size = 1024,
    StargazersCount = 50,
    WatchersCount = 10,
    ForksCount = 5,
    OpenIssuesCount = 2,
    Topics = new List<string?> { "example", "demo" },
    DefaultBranch = "main",
    IsTemplate = false,
    ReadMeContent = "# Example Repo\nThis is an example README content.",
    ReadDate = DateTime.UtcNow
};
```

## Conclusion
The `GitHubRepository` class serves as a comprehensive representation of GitHub repositories, providing all the necessary fields to manage and utilize repository data effectively within the RepoRanger application.
