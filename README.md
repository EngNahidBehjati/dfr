# DataFlowRunner

DataFlowRunner is a .NET 8 backend service that retrieves Pipeline Builder data-flow topologies, maps them to strongly typed graph objects, and keeps snapshots available for downstream orchestration components. It packages reusable pieces such as topology fetching, authentication handling, and in-memory graph caching so other services can focus on orchestration logic rather than integration plumbing.


## Architecture Overview
DataFlowRunner is organised around three composable namespaces:

- `Commons` shared abstractions, dependency registration helpers, option validation, and authentication helpers.
- `DataFlowSnapshot` the topology fetcher, DTOs, and mapper that converts pipeline metadata into domain objects.
- `GraphCore` graph domain models plus a getter that loads from the snapshot fetcher and caches results in `GraphContainer`.

## Project Layout
- `DataFlowRunner/` application code and dependency injection setup.
- `DataFlowRunner/Commons/` shared models, options, and service registrations.
- `DataFlowRunner/DataFlowSnapshot/` DTOs, Refit client interfaces, and the topology mapper/fetcher.
- `DataFlowRunner/GraphCore/` graph domain model, cache (`GraphContainer`), and retrieval logic.
- `DataFlowRunner.Tests/` unit and snapshot tests verifying graph construction and caching behaviour.

## Getting Started

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) (the repository pins 8.0.0 via `global.json`).
- Access to the Pipeline Builder service or a mock that implements the topology endpoint.

### Restore dependencies
```powershell
dotnet restore
```

### Configure the Pipeline Builder endpoint
The Refit client reads the base URL from `Development:DataFlowBuilder:BaseUrl`.

- Update `DataFlowRunner/appsettings.Development.json` for local development.

Configuration is validated at startup; the service will fail fast if the URL is missing or invalid.

### Run the service
```powershell
dotnet run --project DataFlowRunner
```

Swagger is enabled automatically while `ASPNETCORE_ENVIRONMENT=Development`.

## Build and Test
```powershell
dotnet build
dotnet test
```

## Contributing
1. Fork the repository and create a feature branch.
2. Stick to the existing code style (see `.editorconfig`) and favour strongly typed ids when adding new domain objects.
3. Add or update tests where behaviour changes; run `dotnet test` before submitting a pull request.
4. Run `dotnet foramt` before submitting a pull request.
5. Open a PR with a concise description of the change and any follow-up ideas.
