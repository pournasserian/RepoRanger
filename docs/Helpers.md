---
layout: page
title: Helpers Class Documentation
---

# Helpers Class Documentation

## Overview
The `Helpers` class provides a set of utility methods to facilitate the conversion between JSON elements and MongoDB BsonDocuments, along with additional functionalities for managing GitHub repository data within the RepoRanger application.

## Methods

### ConvertToBson(JsonElement item)
- **Description:** Converts a JSON element to a BsonDocument and ensures that the `id` is stored as a long (Int64) and adds an internal timestamp (`internal_created_at`) to the document.
- **Parameters:**
  - `item`: A `JsonElement` representing the JSON data to convert.
- **Returns:** A `BsonDocument` containing the converted data.

### SetReadMeContent(BsonDocument item, string? readmeContent)
- **Description:** Adds a new field named `readme_content` to the BsonDocument, containing the provided README content. If the content is null or whitespace, no action is taken.
- **Parameters:**
  - `item`: A `BsonDocument` to modify.
  - `readmeContent`: A string containing the README content to add.

### ConvertToGitHubRepository(JsonElement json)
- **Description:** Converts a JSON element representing a GitHub repository to a `GitHubRepository` object. It extracts necessary properties from the JSON element to populate the fields of the `GitHubRepository` class.
- **Parameters:**
  - `json`: A `JsonElement` representing the GitHub repository data.
- **Returns:** A `GitHubRepository` object populated with data from the JSON element.

## Examples

### ConvertToBson Example
```csharp
JsonElement jsonElement = // some JSON element;
BsonDocument bsonDocument = jsonElement.ConvertToBson();
```

### SetReadMeContent Example
```csharp
BsonDocument doc = new BsonDocument();
doc.SetReadMeContent("# Sample README Content");
```

### ConvertToGitHubRepository Example
```csharp
JsonElement jsonRepo = // some JSON element for a GitHub repository;
GitHubRepository repository = jsonRepo.ConvertToGitHubRepository();
```

## Conclusion
The `Helpers` class is an integral part of the RepoRanger application, providing the necessary utility methods that streamline data conversion processes and enhance interaction with GitHub repository data.
