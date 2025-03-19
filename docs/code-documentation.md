# RepoRanger Code Documentation

This document provides detailed information about the source code components in the RepoRanger project.

## Table of Contents

1. [Program.cs](#programcs)
2. [GitHubRepository.cs](#githubrepositorycs)
3. [GitHubService.cs](#githubservicecs)
4. [Repository.cs](#repositorycs)
5. [Helpers.cs](#helperscs)

## Program.cs

**Purpose**: Application entry point that configures dependency injection and orchestrates the application workflow.

**Responsibilities**:
- Configure application settings using `appsettings.json`
- Set up dependency injection container
- Register services for GitHub API interaction and MongoDB storage
- Execute the main application flow

**Key Components**:
- Configuration setup from `appsettings.json`
- Dependency injection with `ServiceCollection`
- Service registration with appropriate lifetimes
- Main application logic that:
  1. Searches for repositories
  2. Stores them in MongoDB
  3. Extracts and stores README content

## GitHubRepository.cs

**Purpose**: Data model class that represents a GitHub repository.

**Responsibilities**:
- Define the structure for GitHub repository data
- Provide properties for all relevant repository metadata
- Store README content when extracted

**Key Properties**:
- `Id`: Unique identifier for the repository
- `Name`, `FullName`: Repository name information
- `Description`: Repository description text
- Metadata: Creation date, stars, forks, issues count, etc.
- `ReadMeContent`: Storage for extracted README content

## GitHubService.cs

**Purpose**: Service responsible for interacting with the GitHub API.

**Responsibilities**:
- Search for repositories based on configured criteria
- Extract README content from repositories
- Handle API rate limiting
- Convert API responses to domain objects

**Key Methods**:
- `SearchRepositoriesAsync()`: Search repositories with default or custom criteria
- `ExtractReadmeAsync()`: Fetch and decode README content from a repository
- `GetRepositoryCount()`: Get the total count of matching repositories
- `CallApi()`: Make requests to the GitHub API with rate limit handling
- `RateLimitExceeded()`: Check if the GitHub API rate limit has been reached

**Notable Patterns**:
- Recursive search strategy for handling large result sets
- Automatic date range splitting to stay within API limits
- Rate limit detection and automatic waiting
- Exponential backoff for API calls

## Repository.cs

**Purpose**: Data access layer for storing and retrieving repository data in MongoDB.

**Responsibilities**:
- Connect to MongoDB database
- Insert repository data with upsert semantics
- Update repositories with README content
- Handle MongoDB operations in a thread-safe manner

**Key Components**:
- `IRepository`: Interface defining repository operations
- `MongoDbRepository`: Implementation using MongoDB driver
- `InsertManyAsync()`: Bulk insert repositories with upsert functionality
- `UpdateReadMeAsync()`: Update repositories with extracted README content

**Notable Patterns**:
- Upsert operations to prevent duplicate repositories
- Bulk write operations for performance
- Error handling and logging for database operations

## Helpers.cs

**Purpose**: Utility methods for data conversion and transformation.

**Responsibilities**:
- Convert between JSON and BSON document formats
- Transform API responses to domain objects
- Handle type conversions for MongoDB storage

**Key Methods**:
- `ConvertToBson()`: Convert JSON elements to BSON documents
- `ConvertToGitHubRepository()`: Transform JSON to GitHubRepository objects
- `SetReadMeContent()`: Add README content to BSON documents
- Support functions for consistent data handling
