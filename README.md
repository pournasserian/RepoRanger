# RepoRanger

RepoRanger is an innovative .NET application that empowers developers to seamlessly search for GitHub repositories based on predefined criteria. With the ability to extract complete README content and efficiently store the results in a MongoDB database, RepoRanger transforms the way developers interact with GitHub data.

## Key Features
- **Repository Search**: Discover GitHub repositories that meet specific search criteria, including keywords, star counts, and more.
- **README Extraction**: Automatically extract and store README content from repositories for later reference or analysis.
- **MongoDB Integration**: Effortlessly store and manage repository data in a MongoDB database for robust data handling.
- **Configurable Settings**: Easily adjust connection settings and search parameters in the `appsettings.json` file to meet your project needs.
- **User Notifications**: Implement notifications for users when new repositories match their search criteria.
- **Repository Statistics**: Provide detailed statistics and analyses on searched repositories.

## Configuration
Before you begin using RepoRanger, ensure you have the necessary configuration settings in place for both GitHub and MongoDB. These settings are provided in the `appsettings.json` file located in the `src/RepoRanger` directory.

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
```

## Getting Started
1. **Clone the Repository**: Use `git clone <repository-url>` to clone the project to your local machine.
2. **Install Dependencies**: Ensure you have the latest version of .NET installed. Navigate to the project directory and run `dotnet restore`.
3. **Run the Application**: After restoring dependencies, run the application using `dotnet run` to start the process of searching for repositories!

## Roadmap
To ensure continuous improvement and feature enhancement, the following roadmap outlines planned future updates:
- [ ] Implement additional search filters for repository results.
- [ ] Enhance README extraction with additional formatting options.
- [ ] Add user authentication for GitHub API access.
- [ ] Support for more databases besides MongoDB.
- [ ] Create a user-friendly GUI for easier interaction.
- [ ] Integrate feedback mechanism for user suggestions.
- [ ] Develop advanced filtering options based on repository topics.
- [ ] Create a social sharing option for discovered repositories.

RepoRanger provides a powerful and flexible way to interact with GitHub repositories, making it easier than ever to gather insights and information. Dive in and explore the endless possibilities!