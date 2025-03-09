using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
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

var bsonDocuments = new List<BsonDocument>();

foreach (var item in githubRepos)
{
    var bsonDocument = item.ConvertToBson();
    var readmeContent = await searchService.ExtractReadmeAsync(bsonDocument["full_name"].ToString() ?? null, bsonDocument["default_branch"].ToString() ?? null);
    bsonDocument.SetReadMeContent(readmeContent);
    bsonDocuments.Add(bsonDocument);
}

await repository.InsertManyAsync(bsonDocuments);

