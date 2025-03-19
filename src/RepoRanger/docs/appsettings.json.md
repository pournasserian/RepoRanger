# appsettings.json

## Overview

This file contains the configuration settings for the RepoRanger application. It is a JSON-formatted configuration file that provides essential parameters for connecting to the GitHub API and MongoDB database.

## Structure

The configuration file is structured into two main sections:

1. **GitHubSettings**: Settings related to the GitHub API and repository search criteria
2. **MongoDbSettings**: Settings related to the MongoDB database connection and collection

## Configuration Sections

### GitHubSettings

```json
"GitHubSettings": {
  "ApiToken": "GITHUB_API_TOKEN",
  "Keywords": "fluentcms",
  "MinStars": 10,
  "ShowForked": false,
  "CreatedFrom": "2019-01-01",
  "CreatedTo": "2025-03-08",
  "Language": "C#"
}
```

| Setting | Type | Description | Default (if omitted) |
|---------|------|-------------|----------------------|
| `ApiToken` | string | GitHub API token for authentication (required) | None - will throw exception if missing |
| `Keywords` | string | Search terms for finding repositories | "fluentcms" |
| `MinStars` | integer | Minimum number of stars for repositories | 1 |
| `ShowForked` | boolean | Whether to include forked repositories | false |
| `CreatedFrom` | date string | Start date for repository creation filter (yyyy-MM-dd) | One year ago from current date |
| `CreatedTo` | date string | End date for repository creation filter (yyyy-MM-dd) | Current date |
| `Language` | string | Filter repositories by programming language | null (no language filter) |

### MongoDbSettings

```json
"MongoDbSettings": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "RepoRanger",
  "CollectionName": "Repositories"
}
```

| Setting | Type | Description | Default (if omitted) |
|---------|------|-------------|----------------------|
| `ConnectionString` | string | MongoDB connection string (required) | None - will throw exception if missing |
| `DatabaseName` | string | Name of the MongoDB database (required) | None - will throw exception if missing |
| `CollectionName` | string | Name of the MongoDB collection for storing repositories (required) | None - will throw exception if missing |

## Usage

The application loads this configuration file during startup in the `Program.cs` file:

```csharp
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
```

The settings are then used by:

1. **GitHubService**: Uses the GitHubSettings section to authenticate with the GitHub API and control search parameters
2. **MongoDbRepository**: Uses the MongoDbSettings section to connect to the MongoDB database

## Important Notes

- The `ApiToken` value should be replaced with a valid GitHub API token before running the application
- The MongoDB connection must be properly configured and running at the specified connection string
- The `optional: false` parameter in the configuration loading code indicates that this file is required for the application to run
- The `reloadOnChange: true` parameter enables hot reloading of configuration when the file changes
