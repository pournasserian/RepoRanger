// call GitHub's repository search api to get a list of repositories
// that match the search criteria for AI, ML, and Data Science, LLM, and NLP
// and then call the repository's contributors api to get a list of contributors
// and then call the repository's languages api to get a list of languages
// and then call the repository's topics api to get a list of topics
// and then call the repository's contents api to get a list of files
// and then call the repository's commits api to get a list of commits
// and then call the repository's branches api to get a list of branches
// and then call the repository's releases api to get a list of releases
// and then call the repository's forks api to get a list of forks
// and then call the repository's stargazers api to get a list of stargazers
// and then call the repository's watchers api to get a list of watchers
// and then call the repository's issues api to get a list of issues
// and then call the repository's pull requests api to get a list of pull requests
// and then call the repository's discussions api to get a list of discussions
// and then call the repository's projects api to get a list of projects
// and then call the repository's actions api to get a list of actions


// This function will call the GitHub API to get a list of repositories
// that match the search criteria for AI, ML, and Data Science, LLM, and NLP

using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepoRanger;
using RepoRanger.Models;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.AddAutoMapper(typeof(MappingProfile));
services.AddScoped<IConfiguration>(sp => configuration);
//services.AddScoped<GitHubExplorer>();
services.AddScoped<IRepository, MongoDbRepository>();
services.AddScoped<GitHubSearchService>();

var serviceProvider = services.BuildServiceProvider();
//var mapper = serviceProvider.GetRequiredService<IMapper>();
//var gitHubExplorer = serviceProvider.GetRequiredService<GitHubExplorer>();
var repositoryService = serviceProvider.GetRequiredService<GitHubSearchService>();
var gitHubSearchService = serviceProvider.GetRequiredService<GitHubSearchService>();

var keywords = "deep learning"; // new List<string> { "AI" }; //, "NLP", "Deep Learning", "Machine Learning", "Neural Networks" };
var gitHubRepositories = await gitHubSearchService.SearchRepositoriesAsync(keywords,50,30);
//var repositories = mapper.Map<List<Repository>>(gitHubRepositories);
foreach (var item in gitHubRepositories)
{
    var x = item;
}
//await repositoryService.InsertManyAsync(repositories);

