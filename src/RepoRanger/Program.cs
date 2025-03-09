using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepoRanger;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

services.AddScoped<IConfiguration>(sp => configuration);
services.AddScoped<IRepository, MongoDbRepository>();
services.AddScoped<ISearchService, GitHubService>();

var serviceProvider = services.BuildServiceProvider();
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
