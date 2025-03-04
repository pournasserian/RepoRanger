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
services.AddScoped<ISearchService, GitHubSearchService>();

var serviceProvider = services.BuildServiceProvider();
var repository = serviceProvider.GetRequiredService<IRepository>();
var searchService = serviceProvider.GetRequiredService<ISearchService>();

var keywords = "ai"; // new List<string> { "AI" }; //, "NLP", "Deep Learning", "Machine Learning", "Neural Networks" };
var githubRepos = await searchService.SearchRepositoriesAsync(keywords, 100, false, 400);
await repository.InsertManyAsync(githubRepos);

