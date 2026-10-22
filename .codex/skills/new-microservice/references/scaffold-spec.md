# VAA EAPI Microservice Scaffold Spec

This reference exists to keep repeated `/new-microservice` runs consistent. Prefer editing `scripts/new_microservice.py` when changing generated output; update this spec after script changes.

## Input Parsing

Canonical invocation:

```bash
/new-microservice [microservice domain and name]
```

Accepted examples:

```bash
/new-microservice Retail Pricing
/new-microservice CRM CustomerProfile
/new-microservice FlightOperations Crew
/new-microservice Flight Operations Crew
/new-microservice Ground Operations Passenger Baggage
/new-microservice Retail.Holidays StoreAppointment
/new-microservice Retail Holidays Store Appointment
```

Parsing rules:

- Prefer explicit `--domain` and `--name` if supplied.
- Recognize known domain solution prefixes first, using the longest match.
- Known multi-token domains include `GroundOperations`, `FlightOperations`, and `Retail.Holidays`.
- If no known domain matches, use the first token as domain and the remaining tokens as the microservice name.
- Normalize every namespace segment to PascalCase.
- Preserve all-uppercase acronyms, such as `CRM`.
- Separate service words become separate namespace segments.
- Compound service names stay compound only when the user writes them as compound tokens, such as `AddressLookup` or `StoreAppointment`.

## Project Names

For domain `Retail` and name `Pricing`:

- Project: `VAA.EAPI.Microservices.Retail.Pricing`
- Namespace: `VAA.EAPI.Microservices.Retail.Pricing`
- Tests: `VAA.EAPI.Microservices.Retail.Pricing.Tests`
- Deployment: `VAA.EAPI.Microservices.Retail.Pricing.Deployment`
- Pipeline: `VAA.EAPI.Pipelines/Retail.Pricing.Microservice.CICD.Pipeline.yaml`
- Solution: `VAA.EAPI.Microservices.Retail.sln` when it exists, otherwise `VAA.EAPI.Microservices.sln`

## Function App Files

Main project files:

```text
Application/HelloWorld/GetHelloWorldQuery.cs
Application/HelloWorld/GetHelloWorldQueryHandler.cs
Constants/Application.cs
Constants/Environment.cs
Domain/HelloWorld/HelloWorldRequest.cs
Domain/HelloWorld/HelloWorldResponse.cs
ErrorCodes/ErrorCodes.json
Interfaces/IGetHelloWorldQueryHandler.cs
Interfaces/IHelloWorldMessageService.cs
Services/HelloWorldMessageService.cs
Triggers/Http/HealthController.cs
Triggers/Http/HelloWorldController.cs
Triggers/Timer/.gitkeep
Triggers/ServiceBus/.gitkeep
Program.cs
host.json
local.settings.json
README.md
<Project>.csproj
```

Startup requirements:

- `AddCommandLine(args)`
- `AddEnvironmentConfiguration()`
- `ConfigureFunctionsWorkerDefaults(worker => worker.AddDefaultMiddleware())`
- `ConfigureOpenApi()`
- `AddDefaultServices()`
- `AddHttpClient()`
- `AddSingleton<IHttpConnector, HttpConnector>()`
- Explicit service/handler registrations

Project references and packages:

- Reference `..\VAA.EAPI.Infrastructure\VAA.EAPI.Infrastructure.csproj`.
- Target `net10.0`.
- Use Azure Functions v4 isolated worker.
- Include Application Insights worker telemetry packages.
- Include `Microsoft.Azure.Functions.Worker.Extensions.OpenApi`.
- Include warmup extension for the warmup trigger.

## Trigger Requirements

HTTP triggers live under `Triggers/Http`.

Generate:

- `GET /api/hello?name=...`
- `POST /api/hello`
- `GET /api/heartbeat`
- warmup trigger

Feature endpoints use OAuth2 OpenAPI security but keep the function trigger authorization level anonymous, matching existing APIM-fronted services.

Heartbeat has an OpenAPI operation and returns `HealthReport`.

## OpenAPI and Swagger

The function app must generate swagger from attributes:

- `Program.cs` calls `ConfigureOpenApi()`.
- Function methods include OpenAPI operation, security, parameters/request body, and response attributes.
- DTO properties include `OpenApiProperty` descriptions.
- `local.settings.json` and ARM parameters include:
  - `OpenApi__DocTitle`
  - `OpenApi__Description`
  - `OpenApi__DocVersion`
  - `OpenApi__Version`
  - `OpenApi__TokenUrl`
  - `OpenApi__Scope`
  - `OpenApi__ForceHttps`
  - `OpenApi__ForceHttp`
  - `OpenApi__HostNames`

APIM import should target:

```text
https://<function-app>.azurewebsites.net/api/swagger.json
```

Change the URL only for services that intentionally deploy under a different host or route prefix.

## Tests

Create `<Project>.Tests` with:

- xUnit v3
- coverlet collector
- `Microsoft.NET.Test.Sdk`
- No Moq
- Project reference to the main function app
- Real DI registration through `ServiceCollection`
- At least one happy-path handler test
- At least one fallback/edge handler test
- `.mocks/.gitkeep` for future HTTP payloads

## Deployment Project

Create `VAA.EAPI.Deployments/<Project>.Deployment` with:

```text
Deploy-AzureResourceGroup.ps1
Deployment.targets
Parameters/dev-azuredeploy.parameters.json
Parameters/sit-azuredeploy.parameters.json
Parameters/uat-azuredeploy.parameters.json
Parameters/prod-azuredeploy.parameters.json
Template/azuredeploy.json
Template/azuredeploy_slot.json
Template/azuredeploy_apim.json
<Project>.Deployment.deployproj
```

Deployment templates include:

- Storage account
- Application Insights
- Function app
- Deployment slot for slot pipelines
- OpenAPI app settings
- DNS app settings
- `IPRestriction` parameter
- APIM import from swagger

Parameter files must use placeholders for all environment-specific infrastructure names and secrets. Do not fabricate real values.

## Pipeline Workflow

Create a pipeline YAML under `VAA.EAPI.Pipelines`.

Use existing APIM-aware templates:

- Build: `functions/build_apim.yaml`
- Release: `functions/release_slot_apim.yaml`

Pipeline variables include:

- `Common_Variables`
- `AzureWebJobsStorage`
- `APIM`
- `APIM_IP`
- `Spectral_Ops`
- `applicationName`
- `microserviceName`
- `apimurlname`
- environment-specific function app placeholders
- SIT/UAT sanity test repository placeholders

Pipeline path filters must include both the generated microservice project and deployment project.

## Validation

After generation, run the most relevant commands feasible:

```bash
dotnet build VAA.EAPI.Microservices/<matching-solution>.sln -c Release
dotnet test VAA.EAPI.Microservices/<matching-solution>.sln -c Release
scripts/verify.ps1 -ProjectPath VAA.EAPI.Microservices/<Project>/<Project>.csproj -SkipBuild
```

If full solution build/test is too expensive, run `--dry-run`, inspect the generated files, and run the project-scoped harness command.
