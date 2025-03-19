# RepoRanger Documentation

## Project Overview

RepoRanger is a C# application that searches for GitHub repositories matching specified criteria, extracts their README content, and stores the repository information in a MongoDB database. This tool helps developers discover and catalog repositories of interest for further analysis or reference.

## Key Features

- Search for GitHub repositories based on customizable criteria (keywords, stars, creation date, language)
- Handle GitHub API pagination and rate limiting
- Extract README content from repositories
- Store repository information and README content in MongoDB
- Support for efficient batch processing and data deduplication

## Technical Architecture

RepoRanger follows a clean architecture approach with separation of concerns:

1. **Models**: Representation of GitHub repository data
2. **Services**: Communication with the GitHub API
3. **Repository Layer**: Data persistence with MongoDB
4. **Utilities**: Helper functions for data transformation
5. **Configuration**: External settings for API access and database connections

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│                 │      │                 │      │                 │
│  GitHubService  │──────▶   Program.cs    │──────▶ MongoRepository │
│  (API Access)   │      │  (Orchestrator) │      │ (Data Storage)  │
│                 │      │                 │      │                 │
└─────────────────┘      └─────────────────┘      └─────────────────┘
         │                                                 │
         │                                                 │
         ▼                                                 ▼
┌─────────────────┐                             ┌─────────────────┐
│                 │                             │                 │
│ GitHubRepository│◀────────────────────────────│    MongoDB      │
│     (Model)     │                             │   Database      │
│                 │                             │                 │
└─────────────────┘                             └─────────────────┘
```

## File Documentation

- [Program.cs](Program.cs.md) - Application entry point and workflow orchestration
- [Repository.cs](Repository.cs.md) - MongoDB repository implementation for data persistence
- [GitHubRepository.cs](GitHubRepository.cs.md) - Model representing GitHub repository data
- [GitHubService.cs](GitHubService.cs.md) - Service for communicating with the GitHub API
- [Helpers.cs](Helpers.cs.md) - Utility methods for data conversion and manipulation
- [RepoRanger.csproj](RepoRanger.csproj.md) - Project configuration and dependencies
- [appsettings.json](appsettings.json.md) - Application configuration settings

## Configuration

The application is configured through the [appsettings.json](appsettings.json.md) file, which contains:

1. **GitHubSettings** - Parameters for GitHub API access and search criteria
2. **MongoDbSettings** - Parameters for MongoDB connection and database structure

## Usage

To use RepoRanger:

1. Ensure MongoDB is running at the connection string specified in appsettings.json
2. Replace the `GITHUB_API_TOKEN` placeholder in appsettings.json with a valid GitHub API token
3. (Optional) Adjust search parameters in the GitHubSettings section to match your requirements
4. Run the application using the .NET CLI or an IDE:
   ```
   dotnet run
   ```

## Dependencies

- .NET 8.0
- MongoDB.Driver (3.2.1)
- Microsoft.Extensions.Configuration.Json (9.0.2)
- Microsoft.Extensions.DependencyInjection (9.0.2)
