# Program.cs

## Overview

This file serves as the entry point for the RepoRanger application. It handles the configuration setup, dependency injection, and orchestrates the main workflow of the application.

## Key Functionality

- Sets up configuration from appsettings.json
- Configures dependency injection for the application services
- Executes the main workflow:
  1. Searches for GitHub repositories based on configured criteria
  2. Stores repository data in MongoDB
  3. Extracts README content for each repository
  4. Updates the stored repositories with README content

## Dependencies

- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- RepoRanger services (IRepository, ISearchService)

## Code Explanation

### Configuration Setup

```csharp
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
```

This code initializes the configuration system, setting the base path to the application's directory and loading settings from appsettings.json. The `optional: false` parameter indicates that the file is required, and `reloadOnChange: true` ensures that changes to the file are detected during runtime.

### Dependency Injection

```csharp
var services = new ServiceCollection();

services.AddScoped<IConfiguration>(sp => configuration);
services.AddScoped<IRepository, MongoDbRepository>();
services.AddScoped<ISearchService, GitHubService>();

var serviceProvider = services.BuildServiceProvider();
```

This section configures the dependency injection container:
- Registers the configuration object
- Maps the `IRepository` interface to the `MongoDbRepository` implementation
- Maps the `ISearchService` interface to the `GitHubService` implementation
- Builds the service provider for resolving dependencies

### Main Workflow Execution

```csharp
var repository = serviceProvider.GetRequiredService<IRepository>();
var searchService = serviceProvider.GetRequiredService<ISearchService>();
var githubRepos = await searchService.SearchRepositoriesAsync();

await repository.InsertManyAsync(githubRepos);

foreach (var item in githubRepos)
{
    var readmeContent = await searchService.ExtractReadmeAsync(item.FullName, item.DefaultBranch);
    if (!string.IsNullOrWhiteSpace(readmeContent))
        await repository.UpdateReadMeAsync(item.Id, readmeContent);
}
```

This code:
1. Resolves the `IRepository` and `ISearchService` implementations
2. Calls `SearchRepositoriesAsync()` to fetch repositories from GitHub based on the configured criteria
3. Stores the repositories in MongoDB using `InsertManyAsync()`
4. For each repository:
   - Extracts the README content using `ExtractReadmeAsync()`
   - Updates the stored repository with the README content if available
