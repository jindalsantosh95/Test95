---
name: new-microservice
description: Scaffold a bare-bones VAA EAPI Azure Functions isolated-worker microservice from a domain and service name. Use when the user invokes /new-microservice [microservice domain and name], asks to create a new VAA EAPI microservice, or requests a clean-architecture microservice scaffold with Application, Domain, Interfaces, Services, Triggers, ErrorCodes, tests, deployment project, Azure DevOps pipeline workflow, OpenAPI attributes, swagger generation, and APIM import wiring.
---

# New Microservice

Use this skill to create a consistent VAA EAPI microservice scaffold in the current repository. Prefer the bundled generator over hand-writing files; it is deterministic and encodes the repo conventions.

## Command Form

Treat `/new-microservice [microservice domain and name]` as the canonical invocation.

Examples:

```bash
/new-microservice Retail Pricing
/new-microservice FlightOperations Crew
/new-microservice Ground Operations Passenger Baggage
/new-microservice Retail.Holidays StoreAppointment
```

If the input is ambiguous, prefer running the generator with `--dry-run` and show the resolved namespace before writing files. Use explicit flags when needed:

```bash
python3 .codex/skills/new-microservice/scripts/new_microservice.py --domain Retail.Holidays --name StoreAppointment --repo /path/to/eapi --dry-run
```

## Workflow

1. Confirm the working directory is inside the EAPI repo by locating `VAA.EAPI.Microservices` and `VAA.EAPI.Deployments`.
2. Parse the requested domain/name into namespace segments. Known domain solutions are preferred, including `CRM`, `Data`, `GroundOperations`, `Loyalty`, `Retail`, `Retail.Holidays`, `Utilities`, and `FlightOperations`.
3. Run the generator from this skill:

```bash
python3 .codex/skills/new-microservice/scripts/new_microservice.py [domain and name] --repo /path/to/eapi
```

4. Inspect the generated summary. The namespace must be `VAA.EAPI.Microservices.<Domain>.<Name>` and must include both the domain and service name.
5. Review generated deployment placeholders in `VAA.EAPI.Deployments/<Project>.Deployment/Parameters/*.json`; never invent real Key Vault names, function app names, storage accounts, APIM names, tenant IDs, client IDs, or secrets.
6. Run focused verification when feasible:

```bash
dotnet build VAA.EAPI.Microservices/<matching-solution>.sln -c Release
dotnet test VAA.EAPI.Microservices/<matching-solution>.sln -c Release
scripts/verify.ps1 -ProjectPath VAA.EAPI.Microservices/<Project>/<Project>.csproj -SkipBuild
```

Use solution-scoped commands, not repo-root build/test commands.

## Generated Shape

The generator creates:

- Function app project under `VAA.EAPI.Microservices/VAA.EAPI.Microservices.<Domain>.<Name>`.
- Sibling test project named `<Project>.Tests`.
- Deployment project under `VAA.EAPI.Deployments/<Project>.Deployment`.
- Azure DevOps pipeline YAML under `VAA.EAPI.Pipelines/<Domain>.<Name>.Microservice.CICD.Pipeline.yaml`.
- `Program.cs` with command-line and environment configuration, `ConfigureFunctionsWorkerDefaults`, default middleware, `ConfigureOpenApi()`, `AddDefaultServices()`, `AddHttpClient()`, `IHttpConnector`, and explicit handler/service registrations.
- Clean architecture folders: `Application`, `Constants`, `Domain`, `Interfaces`, `Services`, `Triggers/Http`, `Triggers/Timer`, `Triggers/ServiceBus`, and `ErrorCodes`.
- Hello-world application request/handler, domain DTOs, service interface/implementation, HTTP trigger, heartbeat, warmup trigger, README, `host.json`, `local.settings.json`, and error-code payload.
- xUnit tests with real DI registrations and no Moq.
- ARM deployment files and APIM import template wired to the generated swagger URL.

## OpenAPI Requirements

Generated HTTP triggers must include:

- `ConfigureOpenApi()` in startup.
- `OpenApiOperation` with useful operation IDs, tags, summaries, descriptions, and important visibility.
- `OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))` for feature endpoints.
- `OpenApiRequestBody` for POST request DTOs.
- `OpenApiParameter` for query/path parameters.
- `OpenApiResponseWithBody` for success responses.
- `OpenApiResponseWithoutBody` for standard unauthorized, forbidden, and internal-error responses.
- DTO `OpenApiProperty` descriptions.

Deployment must include OpenAPI app settings and APIM import from `https://<function-app>.azurewebsites.net/api/swagger.json`. Update this URL only if the target service intentionally uses a different host or route prefix.

## Consistency Rules

Always let the script resolve names and write the base scaffold. Do not hand-create parallel naming or folder rules.

Use these naming rules:

- Project: `VAA.EAPI.Microservices.<Domain>.<Name>`.
- Namespace: same as project.
- Test project: `<Project>.Tests`.
- Deployment project: `<Project>.Deployment`.
- Pipeline application name: `<Domain>.<Name>` without the `VAA.EAPI.Microservices` prefix.
- If service words should be one namespace segment, the user must write them as one token, such as `StoreAppointment`; separate words become separate namespace segments.

For deeper details or when updating the generator, read `references/scaffold-spec.md`.
