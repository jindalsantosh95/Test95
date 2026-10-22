# VAA EAPI Platform

## Overview
The VAA EAPI repository contains the Virgin Atlantic EAPI platform codebase. It is organized as multiple .NET solutions grouped by domain so teams can work on a focused slice of the platform without building the entire monorepo. The codebase includes:

- **Core microservices** for retail, CRM, loyalty, flight operations, ground operations, and shared infrastructure.
- **GraphQL gateway** projects that surface domain functionality via queries and mutations.
- **Facade services** that adapt and integrate with external systems (identity, wallet, partner systems).
- **Orchestration services** for cross-domain workflows.
- **Automation and test tooling** for API/UI tests, mocks, and stress test harnesses.
- **Deployment projects** and CI/CD pipeline definitions.

Most services target **.NET 10.0** with some legacy **.NET 5.0** test projects still present.

## Repository structure (what lives where)
| Area | Description | Key solution(s) |
| --- | --- | --- |
| `VAA.EAPI.Automation` | Automated test suites and utilities (UI, API, mock tools, stress tests, test runner services). | `VAA.EAPI.Automation.sln` |
| `VAA.EAPI.Deployments` | Deployment projects and templates for microservices, APIM, and supporting components. | `VAA.EAPI.Deployments.sln` |
| `VAA.EAPI.Facade` | Facade services and adapters (identity, enrolment, wallet, NDC IFG, etc.). | `VAA.Facade.sln` |
| `VAA.EAPI.GraphQL` | GraphQL gateway and domain resolver projects. | `VAA.EAPI.GraphQL.sln`, `VAA.EAPI.GraphQL.Stitch.sln` |
| `VAA.EAPI.HealthStatusApp` | Health status application for monitoring service health. | `VAA.EAPI.HealthStatusApp.sln` |
| `VAA.EAPI.LoungeManagement` | Lounge management services and tests. | `VAA.EAPI.LoungeManagement.sln` |
| `VAA.EAPI.Microservices` | Core microservices and shared libraries for the platform. | `VAA.EAPI.Microservices.sln` plus domain-specific solutions |
| `VAA.EAPI.NDC.Transformation` | NDC offer/order transformation services and tests. | `VAA.EAPI.NDC.Transformation.sln` |
| `VAA.EAPI.Orchestration` | Orchestration services (cross-domain workflows). | `VAA.EAPI.Orchestration.sln` |
| `VAA.EAPI.Pipelines` | CI/CD pipeline definitions (YAML/infra). | N/A |
| `VAA.EAPI.Pipelines.DigitalServices` | Digital services CI/CD pipeline definitions. | N/A |
| `VAA.EAPI.Web` | Web API layer and supporting libraries/tests. | `VAA.EAPI.Web.sln` |

### Solutions at a glance
Use the solution that matches the area you are working on to keep build/test cycles fast:

- **Microservices**: `VAA.EAPI.Microservices/VAA.EAPI.Microservices.sln`
- **GraphQL**: `VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln`
- **Web APIs**: `VAA.EAPI.Web/VAA.EAPI.Web.sln`
- **Automation**: `VAA.EAPI.Automation/VAA.EAPI.Automation.sln`
- **Deployments**: `VAA.EAPI.Deployments/VAA.EAPI.Deployments.sln`

## Getting started
### Prerequisites
- **.NET SDK 10.0** (projects target `net10.0`; `global.json` selects a stable .NET 10 SDK).
- **.NET SDK 5.0** may be required for older test projects (legacy `net5.0` targets).
- Local infrastructure dependencies (databases, queues, secrets) configured through environment variables, user secrets, or appsettings per project.

### Clone the repo
```bash
git clone <repo-url>
cd eapi
```

### Restore dependencies
Restore the solution you want to work on:
```bash
dotnet restore VAA.EAPI.Microservices/VAA.EAPI.Microservices.sln
```

### Build
```bash
dotnet build VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -c Release
```

### Test
```bash
dotnet test VAA.EAPI.Web/VAA.EAPI.Web.sln -c Release
```

> Tip: Prefer solution-scoped builds/tests instead of `dotnet build` from the repo root to avoid pulling in unrelated projects.

## Deep-dive: GraphQL gateway
The GraphQL layer is structured by domain, with each domain living in its own project:

- **Domain projects**: `VAA.EAPI.GraphQL.Domain.*`
- **Main host**: `VAA.EAPI.GraphQL`

When adding a new domain:
1. Create a domain project under `VAA.EAPI.GraphQL.Domain.<DomainName>`.
2. Add query and mutation resolvers in that project.
3. Add a project reference to `VAA.EAPI.GraphQL`.
4. Register the resolver types in `Program.cs` within the GraphQL host.

The gateway expects resolvers to extend the base `Query` and `Mutation` types and uses HotChocolate filtering/sorting where needed.

## Deep-dive: Microservices
The microservices solution contains domain-specific services and shared libraries. Typical patterns include:

- **Domain services** (e.g., CRM, Loyalty, Retail, Flight Operations, Ground Operations).
- **Shared infrastructure** (common libraries, utilities, security).
- **Tests** co-located with the microservice or in a corresponding `*.Tests` project.

Many microservice folders include multiple service variants (core service, notification worker, background processors, etc.) which are grouped in the same domain folder.

## Deep-dive: Automation & testing
Automation projects include:

- API and UI test suites.
- Http mockers and utilities for integration testing.
- Stress test harnesses and test runner services.

If you are making API or UI changes, check for related automation projects in `VAA.EAPI.Automation` for coverage.

## Knowledge repo
The knowledge repo and harness documentation live in the root `docs` folder:

- [`docs/readme.md`](docs/readme.md)
- [`docs/knowledge-repo-guide.md`](docs/knowledge-repo-guide.md)
- [`docs/knowledge-repo.md`](docs/knowledge-repo.md)

## Deep-dive: Deployments & pipelines
Deployment projects define the release artifacts and infrastructure for the services. Pipeline definitions live under:

- `VAA.EAPI.Pipelines`
- `VAA.EAPI.Pipelines.DigitalServices`

If you update deployment configurations or add a new service, ensure the relevant deployment project and pipeline definitions are updated.

## Local development tips
- **Start small**: build one solution at a time.
- **Use configuration files**: many services are driven by `appsettings*.json` and environment variables.
- **Keep secrets out of source control**: use user secrets or environment variables for local dev.
- **Check test projects**: several domains include dedicated `*.Tests` projects; run them as needed.

## Contributing
1. Pick the solution folder that matches your change (for example `VAA.EAPI.Microservices` or `VAA.EAPI.GraphQL`).
2. Keep changes scoped to the target solution to reduce build/test time.
3. Run `dotnet format` or your team’s linting/formatting process if applicable.
4. Open a PR with a clear description of scope, impacted services, and test coverage.

## Support / Where to start
If you are unsure which solution to use, start with:
- `VAA.EAPI.Microservices.sln` for backend domain services.
- `VAA.EAPI.GraphQL.sln` for GraphQL gateway work.
- `VAA.EAPI.Web.sln` for web API endpoints.
