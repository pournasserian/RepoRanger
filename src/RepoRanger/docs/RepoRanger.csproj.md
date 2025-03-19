# RepoRanger.csproj

## Overview

This file defines the project configuration for the RepoRanger application. It specifies the project type, target framework, compilation settings, file inclusions, and external dependencies.

## Key Components

### Project Properties

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

- **OutputType**: Specifies that the project compiles to an executable (Exe)
- **TargetFramework**: Targets .NET 8.0
- **ImplicitUsings**: Enables implicit using directives, reducing the need for common namespace imports
- **Nullable**: Enables nullable reference types, improving null-safety in the codebase

### File Management

```xml
<ItemGroup>
  <None Remove="appsettings.json" />
</ItemGroup>

<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

These sections manage how project files are handled:
- The first `ItemGroup` removes appsettings.json from the default build process
- The second `ItemGroup` explicitly includes appsettings.json as Content and ensures it's always copied to the output directory when building

### Package References

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.2" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.2" />
  <PackageReference Include="MongoDB.Bson" Version="3.2.1" />
  <PackageReference Include="MongoDB.Driver" Version="3.2.1" />
</ItemGroup>
```

This section defines the external NuGet packages the project depends on:

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Extensions.Configuration.Json | 9.0.2 | JSON configuration provider for Microsoft.Extensions.Configuration |
| Microsoft.Extensions.DependencyInjection | 9.0.2 | Dependency injection container |
| MongoDB.Bson | 3.2.1 | BSON serialization library for MongoDB |
| MongoDB.Driver | 3.2.1 | MongoDB driver for interacting with MongoDB databases |

## Usage

The project file is used by the .NET SDK to build and manage the application. It can be used with .NET CLI commands such as:

```bash
dotnet build       # Build the project
dotnet run         # Run the application
dotnet publish     # Create a publishable version of the application
```
