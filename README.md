# RepoRanger

RepoRanger is a .NET application that searches for GitHub repositories based on specific criteria and stores the results in a MongoDB database. It also extracts and stores the README content of the repositories.

## Configuration

The application requires configuration settings to connect to GitHub and MongoDB. These settings are provided in the `appsettings.json` file located in the `src/RepoRanger` directory.

```json
{
  "GitHubSettings": {
    "ApiToken": "GITHUB_API_TOKEN",
    "Keywords": "fluentcms",
    "MinStars": 10,
    "ShowForked": false,
    "CreatedFrom": "2019-01-01",
    "CreatedTo": "2025-03-08",
    "Language": "C#"
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "RepoRanger",
    "CollectionName": "Repositories"
  }
}