using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepoRanger;

// Load application configuration from appsettings.json
// This provides GitHub API settings and MongoDB connection details
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Set up the dependency injection container
var services = new ServiceCollection();

// Register services with appropriate scopes
services.AddScoped<IConfiguration>(sp => configuration);
services.AddScoped<IRepository, MongoDbRepository>();    // Data repository
services.AddScoped<ISearchService, GitHubService>();     // GitHub API service

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Resolve required services
var repository = serviceProvider.GetRequiredService<IRepository>();
var searchService = serviceProvider.GetRequiredService<ISearchService>();
// Step 1: Search for GitHub repositories based on the criteria in appsettings.json
// This fetches repositories matching keywords, stars, dates, and language filters
var githubRepos = await searchService.SearchRepositoriesAsync();

// Step 2: Store all found repositories in MongoDB with upsert semantics
// This ensures repositories are created or updated without duplicates
await repository.InsertManyAsync(githubRepos);

// Step 3: Process each repository to extract README content
// This fetches the README file content for each repository and stores it
foreach (var item in githubRepos)
{
    // Extract README content from the repository's default branch
    var readmeContent = await searchService.ExtractReadmeAsync(item.FullName, item.DefaultBranch);

    // Update the repository record with README content if available
    if (!string.IsNullOrWhiteSpace(readmeContent))
        await repository.UpdateReadMeAsync(item.Id, readmeContent);
}
