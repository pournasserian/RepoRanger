# RepoRanger Class Diagram

This document provides a visual representation of the RepoRanger class structure and relationships.

```mermaid
classDiagram
    class Program {
        +Main() void
    }
    
    class GitHubRepository {
        +long Id
        +string Name
        +string FullName
        +string HtmlUrl
        +string Description
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +DateTime PushedAt
        +string Language
        +string HomePage
        +long Size
        +int StargazersCount
        +int WatchersCount
        +int ForksCount
        +int OpenIssuesCount
        +List~string~ Topics
        +string DefaultBranch
        +bool IsTemplate
        +string ReadMeContent
        +DateTime ReadDate
    }
    
    class ISearchService {
        <<interface>>
        +SearchRepositoriesAsync() Task~IEnumerable~GitHubRepository~~
        +SearchRepositoriesAsync(string, int, bool, DateTime, DateTime, string) Task~IEnumerable~GitHubRepository~~
        +ExtractReadmeAsync(string, string) Task~string~
    }
    
    class GitHubService {
        -HttpClient _httpClient
        -string _apiToken
        -IConfiguration _configuration
        +GitHubService(IConfiguration)
        +SearchRepositoriesAsync() Task~IEnumerable~GitHubRepository~~
        +SearchRepositoriesAsync(string, int, bool, DateTime, DateTime, string) Task~IEnumerable~GitHubRepository~~
        +ExtractReadmeAsync(string, string) Task~string~
        -GetRepositoryCount(string, int, bool, DateTime, DateTime, string) Task~int~
        -CallApi(string) Task~JsonDocument~
        -CreateSearchQuery(string, DateTime, DateTime, int, bool, string, int, int) string
        -RetrieveRepositoriesAsync(string, int, bool, DateTime, DateTime, string) Task~IEnumerable~GitHubRepository~~
        -RateLimitExceeded(HttpResponseMessage, out TimeSpan) bool
        -DecodeReadmeContent(string, string) string
    }
    
    class IRepository {
        <<interface>>
        +InsertManyAsync(IEnumerable~BsonDocument~) Task
        +InsertManyAsync(IEnumerable~GitHubRepository~) Task
        +UpdateReadMeAsync(long, string) Task
    }
    
    class MongoDbRepository {
        -IMongoCollection~BsonDocument~ _repositoriesCollection
        -IMongoCollection~GitHubRepository~ _githubCollection
        -MongoClient _client
        +MongoDbRepository(IConfiguration)
        +InsertManyAsync(IEnumerable~BsonDocument~) Task
        +InsertManyAsync(IEnumerable~GitHubRepository~) Task
        +UpdateReadMeAsync(long, string) Task
    }
    
    class Helpers {
        <<static>>
        +ConvertToBson(JsonElement) BsonDocument
        +SetReadMeContent(BsonDocument, string) void
        +ConvertToGitHubRepository(JsonElement) GitHubRepository
        -AddInternalCreatedAt(BsonDocument) void
        -SetIdAsLong(BsonDocument) void
    }
    
    Program --> ISearchService : uses
    Program --> IRepository : uses
    GitHubService ..|> ISearchService : implements
    MongoDbRepository ..|> IRepository : implements
    GitHubService --> GitHubRepository : creates
    MongoDbRepository --> GitHubRepository : stores
    GitHubService --> Helpers : uses
    MongoDbRepository --> Helpers : uses
```

## Key Relationships

1. **Program** is the entry point that coordinates the application flow:
   - Configures dependency injection
   - Uses ISearchService to find repositories
   - Uses IRepository to store data

2. **GitHubService** implements the ISearchService interface:
   - Handles GitHub API communication
   - Searches for repositories based on criteria
   - Extracts README content
   - Manages API rate limiting

3. **MongoDbRepository** implements the IRepository interface:
   - Connects to MongoDB
   - Stores repository data
   - Updates repositories with README content

4. **Helpers** provides utility methods:
   - Converts between JSON and BSON formats
   - Transforms API responses to domain objects
   - Handles type conversions for database storage

5. **GitHubRepository** is the core domain model:
   - Represents GitHub repository data
   - Stores metadata and README content
