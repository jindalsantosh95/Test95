# Coding Standards

**Virgin Atlantic EAPI Platform**

These standards apply to all new and materially changed EAPI code. Existing services may not be fully consistent; do not refactor unrelated legacy code just to make it match this document. When an AI agent changes the EAPI codebase, it must use these standards together with the documents in `docs/principles`.

The examples below show the intended shape of new code. Adapt names, namespaces, response helpers, and exact method signatures to the owning project rather than copying snippets blindly.

## Core Rules

- Write code that is easy to review, test, operate, and safely change.
- Keep changes inside the owning solution and service boundary.
- Prefer existing EAPI platform helpers, response models, middleware, connectors, and telemetry patterns over new local mechanisms.
- Keep orchestration in application handlers, not in triggers, controllers, GraphQL resolvers, or infrastructure clients.
- Keep secrets, credentials, tokens, connection strings, customer PII, and raw sensitive payloads out of source control, logs, exceptions, and documentation.
- Do not add broad refactors, formatting churn, or dependency upgrades unless they are needed for the requested change.

## Language And Runtime

- Use C# and the target framework already used by the owning project.
- New microservices must target supported .NET LTS, currently .NET 10.
- .NET 5 projects are legacy-only. Do not use them as templates for new work.
- Prefer nullable-safe code. Avoid null-forgiving suppressions unless the invariant is obvious and local.
- For collection existence and emptiness checks, prefer the `Count` or `Length` property equally (for example, `collection.Count > 0`, `collection.Count == 0`, `collection.Length > 0`, or `collection.Length == 0`). If neither property is available, use `Any()` or `!Any()`. Use the `Count()` method only when neither property nor `Any()` is available.
- Do not ignore compiler warnings introduced by the change.
- Use solution-scoped commands such as `dotnet restore <solution>.sln`, `dotnet build <solution>.sln -c Release`, and `dotnet test <solution>.sln -c Release`.

Example:

```bash
dotnet restore VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.sln
dotnet build VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.sln -c Release
dotnet test VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.sln -c Release
```

## Repository And Solution Scope

- Work in the solution folder that owns the change, for example `VAA.EAPI.Microservices`, `VAA.EAPI.GraphQL`, `VAA.EAPI.Web`, `VAA.EAPI.Facade`, `VAA.EAPI.NDC.Transformation`, or `VAA.EAPI.Orchastration`.
- Do not build or test from the repository root unless there is a specific reason.
- Update deployment and pipeline files only when runtime behavior, configuration, infrastructure, or release wiring changes.
- Keep generated artifacts, local settings, secrets, and transient outputs out of commits.

## Project Structure

New microservices must follow this structure unless an approved platform pattern says otherwise:

```text
<ProjectName>/
  Application/
  Constants/
  Domain/
  ErrorCodes/
  Interfaces/
  Services/
  Triggers/
    Http/
    ServiceBus/
    Timer/
  Program.cs
  host.json
  README.md
  <ProjectName>.csproj
```

- Put command/request models and handlers in `Application/`.
- Put API and domain DTOs in `Domain/`, grouped by feature.
- Put service and handler interfaces in `Interfaces/`.
- Put interface implementations in `Services/`.
- Put each top-level class, record, struct, enum, and interface in its own `.cs` file named after that type. Nested types may remain with their owning type when they are genuine implementation details.
- Put all Azure Functions in `Triggers/`, grouped by trigger type.
- Put environment variable names in `Constants/Environment.cs`.
- Put reusable application constants in `Constants/Application.cs` or the established local equivalent.
- Put error code payloads in `ErrorCodes/`.
- Add a sibling test project named `<ProjectName>.Tests`.
- Add or update the service `README.md` whenever setup, configuration, endpoints, triggers, or operational behavior changes.

## New Microservice Scaffolding

Use the repo-local Codex skill for repeatable new microservice scaffolding:

```bash
python3 .codex/skills/new-microservice/scripts/new_microservice.py <domain and name> --repo .
```

New microservices must include:

- Azure Functions isolated worker using `Microsoft.Azure.Functions.Worker`.
- .NET 10 target framework.
- OpenAPI-enabled HTTP triggers and swagger generation through function attributes.
- Dependency injection configured in `Program.cs`.
- Caching and connector-based external integration patterns where downstream calls are needed.
- Domain-focused request/handler pattern without introducing MediatR or AutoMapper.
- `Application/`, `Constants/`, `Domain/`, `Interfaces/`, `Services/`, `Triggers/Http`, `Triggers/Timer`, `Triggers/ServiceBus`, and `ErrorCodes/`.
- Interface contracts in `Interfaces/` and implementations in `Services/`.
- HTTP triggers with OpenAPI operation, security, request, and response attributes.
- Heartbeat endpoint and warmup trigger where applicable.
- Unit test project with at least one happy path and one failure or edge scenario.
- Service `README.md` for developer quick start.
- Matching deployment project beside other deployment projects unless a skip reason is documented.

New microservice startup must explicitly confirm:

- `AddDefaultServices()` or equivalent baseline registrations.
- `IHttpConnector` registration and required HTTP client factory support.
- Health checks.
- Application Insights worker telemetry and Functions Application Insights configuration.
- Memory cache and response compression where relevant.
- Default Functions worker middleware before service-specific middleware.

Example:

```text
VAA.EAPI.Microservices.Retail.Order.Refunds/
  Application/
    CreateRefundCommand.cs
    CreateRefundCommandHandler.cs
  Constants/
    Environment.cs
  Domain/
    RefundRequest.cs
    RefundResponse.cs
  Interfaces/
    ICreateRefundCommandHandler.cs
    IRefundGateway.cs
  Services/
    RefundGateway.cs
  Triggers/
    Http/
      RefundController.cs
    ServiceBus/
      RefundStatusSubscriber.cs
  ErrorCodes/
    errors.json
  Program.cs
  host.json
  README.md
```

## Naming

- Use `VAA.EAPI...` namespaces that match the owning project and folder intent.
- Use PascalCase for public types, methods, properties, constants, and enum values.
- Use camelCase for parameters and local variables.
- Prefix interfaces with `I`.
- Suffix asynchronous methods with `Async` unless implementing an interface that already defines another name.
- Use names that describe the business action, not the transport detail. For example, prefer `RefundNotificationPublisher` over `ServiceBusSender`.
- Centralize environment keys and do not duplicate literal setting names across handlers and services.
- Keep JSON contracts stable. Match existing casing and `Newtonsoft.Json` serialization behavior for the owning API.
- Fix obvious typos in newly created code. Avoid renaming public legacy contracts unless the change is planned and versioned.

Good:

```csharp
public interface IRefundNotificationPublisher
{
    Task PublishAsync(RefundNotification notification, CancellationToken cancellationToken);
}

public sealed class RefundNotificationPublisher : IRefundNotificationPublisher
{
    public Task PublishAsync(RefundNotification notification, CancellationToken cancellationToken)
    {
        // Implementation omitted.
        return Task.CompletedTask;
    }
}
```

Avoid:

```csharp
public class busSender
{
    public void DoIt(object data)
    {
        // Unclear responsibility, sync shape, weak contract.
    }
}
```

## JSON Serialization

- JSON serialization and deserialization must use `Newtonsoft.Json` (directly or via approved shared helpers in `VAA.EAPI.Common`/`VAA.EAPI.Infrastructure`).
- Do not introduce `System.Text.Json` serializers, options, or convenience APIs in feature code.
- Keep serializer settings and converters consistent with the owning service contract. Reuse existing shared settings before adding new local behavior.
- Keep request and response contract casing and null-handling unchanged unless the contract versioning plan explicitly allows it.

## Azure Functions And Triggers

- New microservices must use Azure Functions isolated worker with `Microsoft.Azure.Functions.Worker`.
- HTTP triggers must include OpenAPI metadata, request body documentation, response documentation, and the applicable auth/security scheme.
- Triggers must be thin: parse input, establish context, call an application handler, and translate the result.
- Do not put business rules, downstream orchestration, mapping-heavy logic, or retry workflows directly in triggers.
- Include a heartbeat or health endpoint where the service exposes HTTP endpoints.
- Include warmup and timer triggers only when the service has a real startup or scheduled workload requirement.
- Service Bus triggers must deserialize into a clear message/event model, set correlation context, validate the event, and hand off to an application handler.

Good trigger shape:

```csharp
public sealed class RefundController(ICreateRefundCommandHandler handler)
{
    [Function(nameof(CreateRefundAsync))]
    [OpenApiOperation(operationId: "CreateRefund", tags: new[] { "Refunds" })]
    [OpenApiRequestBody("application/json", typeof(CreateRefundCommand))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RefundResponse))]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(List<ErrorDetails>))]
    public async Task<HttpResponseData> CreateRefundAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "refunds")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        CreateRefundCommand? command;

        try
        {
            using var reader = new StreamReader(request.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);
            command = JsonConvert.DeserializeObject<CreateRefundCommand>(requestBody);
        }
        catch (Newtonsoft.Json.JsonException)
        {
            return await request.CreateErrorResponseAsync(HttpStatusCode.BadRequest, "Malformed JSON body.");
        }

        if (command is null)
        {
            return await request.CreateErrorResponseAsync(HttpStatusCode.BadRequest, "Request body is required.");
        }

        var result = await handler.HandleAsync(command, cancellationToken);

        return await ResultExtension<RefundResponse>.ExecuteResultAsync(request, result);
    }
}
```

Avoid putting orchestration in the trigger:

```csharp
public async Task<HttpResponseData> CreateRefundAsync(HttpRequestData request)
{
    var token = await GetTokenAsync();
    var air4Result = await CallAir4Async(token);
    var itaResult = await CallItaAsync(air4Result);
    var response = ApplyBusinessRules(itaResult);

    return await request.CreateJsonResponseAsync(response);
}
```

## Startup And Dependency Injection

- Configure command-line and environment configuration providers in `Program.cs`.
- Use `ConfigureFunctionsWorkerDefaults(...)` and existing default middleware extensions where available.
- Use `ConfigureOpenApi()` for function apps that expose HTTP endpoints.
- Register dependencies explicitly in DI.
- Use `AddDefaultServices()` where available, or explicitly register equivalent platform services.
- New microservice startup must include health checks, Application Insights worker telemetry, function Application Insights configuration, and memory cache when used.
- Register `IHttpConnector` with `services.AddSingleton<IHttpConnector, HttpConnector>()` and ensure `services.AddHttpClient()` is available where the connector needs `IHttpClientFactory`.
- Choose lifetimes deliberately:
  - Singleton only for stateless, thread-safe services.
  - Scoped or transient where work must be bounded to an operation.
  - Avoid static mutable state.
- Do not use service locator patterns to fetch dependencies inside feature code.

Example startup shape:

```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseDefaultMiddleware();
    })
    .ConfigureOpenApi()
    .ConfigureServices(services =>
    {
        services.AddDefaultServices();
        services.AddHealthChecks();
        services.AddMemoryCache();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpClient();

        services.AddSingleton<IHttpConnector, HttpConnector>();
        services.AddScoped<ICreateRefundCommandHandler, CreateRefundCommandHandler>();
        services.AddScoped<IRefundGateway, RefundGateway>();
    })
    .Build();

host.Run();
```

Avoid:

```csharp
public sealed class CreateRefundCommandHandler
{
    public Task HandleAsync(CreateRefundCommand command)
    {
        var gateway = ServiceProviderHolder.Current.GetRequiredService<IRefundGateway>();
        return gateway.CreateAsync(command);
    }
}
```

## Outbound HTTP

- Application and business code must use `IHttpConnector` for outbound HTTP.
- Do not create `HttpClient`, store static clients, or inject `IHttpClientFactory` directly into feature handlers.
- Direct `HttpClient`, typed clients, or SDK-style clients are acceptable only for established infrastructure clients that intentionally sit below the feature layer.
- Pass connector arguments consistently:
  - `uri` is the fully composed downstream endpoint.
  - `headers` contains request-specific headers only.
  - `bearerToken` contains the token value; let the connector set or overwrite `Authorization`.
- Let `HttpConnector` own standard headers, request construction, logging, tracing, and error translation.
- Apply timeouts, retries, and circuit-breaking through shared platform mechanisms where available.
- Do not log bearer tokens, API keys, cookies, full card data, passport data, or raw sensitive request/response payloads.

Good:

```csharp
public sealed class RefundGateway(IHttpConnector httpConnector, IConfiguration configuration) : IRefundGateway
{
    public async Task<RefundGatewayResponse> CreateRefundAsync(
        RefundGatewayRequest request,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        var uri = $"{configuration[Environment.RefundApiBaseUrl]}/refunds";
        var headers = new Dictionary<string, string>
        {
            ["x-correlation-id"] = AsyncContext.Current.CorrelationId.ToString()
        };
        var json = Json.Serialize(request);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        return await httpConnector.PostAsync<RefundGatewayResponse>(
            uri,
            content,
            headers,
            bearerToken);
    }
}
```

Avoid:

```csharp
public sealed class RefundGateway
{
    private static readonly HttpClient Client = new();

    public async Task<string> CreateRefundAsync(string uri, string token, string payload)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await Client.PostAsync(uri, new StringContent(payload)).Result.Content.ReadAsStringAsync();
    }
}
```

## GraphQL

- GraphQL is the orchestration/API composition layer. Keep domain behavior in the relevant domain project.
- Add or update resolver/query/mutation types in the owning domain project.
- Register new GraphQL types, resolvers, services, and middleware in the GraphQL host `Program.cs` or the established host initialization pattern.
- For `VAA.EAPI.GraphQL`, prefer domain-level `Startup : IStartup` registrations (`RegisterQueries`/`RegisterMutations`) instead of direct host-level type wiring for domain capabilities.
- Resolvers must be thin and must call services or handlers for business workflows.
- Avoid direct downstream HTTP calls from resolver code unless the existing domain pattern specifically requires it.
- Preserve schema compatibility unless a breaking change has been approved and versioned.
- Keep REST router (`VAA.EAPI.GraphQL.Rest`) and MCP exposure aligned with registered query/mutation contracts; avoid separate business logic in router functions.
- When adding GraphQL operations that are consumed through REST router or MCP, update route/capability documentation surfaces and associated tests in the same change.

Good:

```csharp
public sealed class RefundQueries
{
    public Task<RefundStatus> GetRefundStatusAsync(
        string refundReference,
        [Service] IRefundStatusService refundStatusService,
        CancellationToken cancellationToken)
    {
        return refundStatusService.GetStatusAsync(refundReference, cancellationToken);
    }
}
```

Avoid:

```csharp
public sealed class RefundQueries
{
    public Task<RefundStatus> GetRefundStatusAsync(string refundReference)
    {
        // Resolver now owns downstream transport, token handling, mapping, and errors.
        return CallDownstreamRefundApiAsync(refundReference);
    }
}
```

## Application Handlers And Mapping

- Use domain-focused request/handler patterns for new microservice work.
- Keep orchestration, validation sequencing, mapping, and downstream calls in application handlers or services, not in triggers.
- Do not introduce MediatR or AutoMapper into new microservices.
- Existing MediatR usage may be maintained inside services that already use it, but do not spread it into new service scaffolding.
- Map objects explicitly so transformations are visible and reviewable.
- Keep mapping code close to the feature when it is feature-specific. Extract shared mappers only when reuse is real.
- Use guard clauses for required dependencies and invalid inputs.

Good handler shape:

```csharp
public sealed class CreateRefundCommandHandler(
    IRefundGateway refundGateway,
    ILogger<CreateRefundCommandHandler> logger) : ICreateRefundCommandHandler
{
    public async Task<Response<RefundResponse>> HandleAsync(
        CreateRefundCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.OrderReference.IsNullOrWhiteSpace())
        {
            return ResponseFactory.BadRequest<RefundResponse>("OrderReference is required.");
        }

        var gatewayRequest = new RefundGatewayRequest
        {
            OrderReference = command.OrderReference,
            Amount = command.Amount,
            Currency = command.Currency
        };

        var gatewayResponse = await refundGateway.CreateRefundAsync(
            gatewayRequest,
            command.BearerToken,
            cancellationToken);

        logger.LogInformation(
            "Refund created for {OrderReference} with {RefundReference}",
            command.OrderReference,
            gatewayResponse.RefundReference);

        return ResponseFactory.Ok(new RefundResponse
        {
            RefundReference = gatewayResponse.RefundReference,
            Status = gatewayResponse.Status
        });
    }
}
```

## Async And Cancellation

- Use async APIs for all I/O.
- Propagate `CancellationToken` from triggers and handlers into downstream calls.
- Do not block async work with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` in application code.
- Avoid fire-and-forget tasks. If background work is required, use a durable trigger, queue, Service Bus message, or scheduled process.
- Do not swallow cancellation. Let `OperationCanceledException` flow unless a controlled response is explicitly required.

Good:

```csharp
public async Task<RefundStatus> GetStatusAsync(string reference, CancellationToken cancellationToken)
{
    var request = new HttpRequestMessage(HttpMethod.Get, $"refunds/{reference}");
    return await httpConnector.SendAsync<RefundStatus>(request, cancellationToken);
}
```

Avoid:

```csharp
public RefundStatus GetStatus(string reference)
{
    return httpConnector.GetAsync<RefundStatus>($"refunds/{reference}").Result;
}
```

## Validation And Null Handling

- Validate external inputs at the boundary and validate business invariants inside the handler/service that owns the rule.
- Fail fast for missing required values, invalid identifiers, unsupported states, and malformed events.
- Prefer explicit result or response models over returning null for failure.
- Do not add try/catch around imports/usings.
- Keep validation error responses consistent with the owning service and shared EAPI error format.
- Before choosing a native .NET API or writing equivalent helper logic, inspect the owning project and its `ProjectReference` graph for applicable repository extension classes. Search the owning and referenced projects for `*Extension*.cs`, `Extensions/`, and the legacy `Extentions/` folder. Prefer an available repository extension when it provides the required semantics, including HTTP content/message/header, dictionary, object, byte, logging, function-context, configuration, test-fixture, and domain-specific helpers. Extension availability is solution/project dependent: do not add a cross-solution dependency merely to obtain an extension.
- Shared string extensions are one concrete instance of this general rule. When the owning project references them, use `(memberInfo?.Id).IsNullOrWhiteSpace()` instead of `string.IsNullOrWhiteSpace(memberInfo?.Id)`, and use `claim.Type.Is(expectedType)` instead of `string.Equals(..., StringComparison.OrdinalIgnoreCase)`. These two native patterns are currently blocked mechanically by the preferred-extension harness sensor; other extension choices are enforced through generation guidance, review, and the owning scope's tests until dedicated detectors are added.

Example:

```csharp
if (request is null)
{
    return ResponseFactory.BadRequest<RefundResponse>("Request body is required.");
}

if (request.Amount <= 0)
{
    return ResponseFactory.BadRequest<RefundResponse>("Refund amount must be greater than zero.");
}
```

## Regular Expressions

- Prefer the owning platform's shared `RegexCompiled` helper for regex matching, replacement, splitting, and capture extraction. Implementations are available in the shared Automation, GraphQL, Microservices, and Web libraries.
- Pass additional `RegexOptions` to `RegexCompiled` when required; the helper applies `RegexOptions.Compiled` consistently.
- Use `RegexCompiled.Try` when a pattern can be supplied dynamically or invalid input must produce a controlled failure instead of throwing.
- Avoid new calls to static `Regex.Match`, `Regex.Matches`, `Regex.IsMatch`, `Regex.Replace`, or `Regex.Split`, and avoid creating a `Regex` solely to execute one of those operations when the owning project can reference its shared helper.
- A dedicated `Regex` instance or source-generated regex remains appropriate when an API requires a `Regex` object, source generation is already the owning project's established pattern, or the shared helper cannot express the required behavior. Keep reusable instances static and document the reason when it is not evident.
- `Regex.Escape` and `Regex.Unescape` are not expression execution and may continue to use the framework APIs.

Good:

```csharp
if (RegexCompiled.IsMatch(accountNumber, @"^\d{1,20}$", RegexOptions.CultureInvariant))
{
    return accountNumber;
}

if (!RegexCompiled.Try.Match(input, configuredPattern, out Match match))
{
    return ResponseFactory.BadRequest<string>("The configured pattern or input is invalid.");
}
```

Avoid:

```csharp
if (Regex.IsMatch(accountNumber, @"^\d{1,20}$", RegexOptions.Compiled))
{
    return accountNumber;
}

var regex = new Regex(configuredPattern, RegexOptions.Compiled);
return regex.Match(input).Value;
```

## Error Handling

- Use established EAPI response and error models such as `Response<T>`, `ErrorDetails`, result wrappers, and error code JSON where the owning project uses them.
- Return controlled client errors for validation, authorization, not found, and conflict cases.
- Return controlled technical errors for unhandled downstream or platform failures.
- Do not leak stack traces, connection details, tokens, query strings containing secrets, or downstream raw error payloads to clients.
- Catch exceptions only when adding useful context, mapping to the service error contract, or performing required cleanup.
- Preserve correlation IDs in errors and logs.
- Prefer fail-closed behavior for authentication, authorization, policy decisions, validation, and downstream uncertainty.

Good:

```csharp
try
{
    return await refundGateway.CreateRefundAsync(request, bearerToken, cancellationToken);
}
catch (DownstreamTimeoutException exception)
{
    logger.LogError(
        exception,
        "Refund downstream timed out for {OrderReference} and {CorrelationId}",
        request.OrderReference,
        AsyncContext.Current.CorrelationId);

    return ResponseFactory.TechnicalError<RefundResponse>(
        ErrorCodes.RefundDownstreamUnavailable,
        AsyncContext.Current.CorrelationId.ToString());
}
```

Avoid:

```csharp
catch (Exception exception)
{
    return ResponseFactory.BadRequest<RefundResponse>(exception.ToString());
}
```

## Logging And Telemetry

- Use `ILogger<T>` with structured logging.
- Prefer message templates and named properties over string interpolation for operational logs.
- Include correlation, operation, session, channel, and event identifiers where available through `AsyncContext` or platform context helpers.
- Log the decision and outcome, not entire sensitive payloads.
- Use appropriate levels:
  - Debug for local diagnostic detail.
  - Information for meaningful business or workflow milestones.
  - Warning for recoverable unusual states.
  - Error for failed operations that need attention.
- Do not log secrets, tokens, credentials, payment card data, passport details, full customer profiles, or unnecessary PII.
- Add telemetry for new external dependencies, message flows, health checks, and important failure paths.

Good:

```csharp
logger.LogInformation(
    "Refund request accepted for {OrderReference} and {CorrelationId}",
    command.OrderReference,
    AsyncContext.Current.CorrelationId);
```

Avoid:

```csharp
logger.LogInformation($"Refund payload: {Json.Serialize(command)} token: {command.BearerToken}");
```

## Security And Privacy

- Implement the security principles in `docs/principles/security-principles.md`.
- Enforce authentication and authorization at the API boundary and again inside sensitive workflows where business rules require it.
- Treat internal networks and internal callers as untrusted unless identity, policy, and authorization have been verified.
- Store secrets in Key Vault or approved managed secret stores. Do not put secrets in `appsettings*.json`, source, tests, docs, or pipeline variables committed to the repo.
- Use managed identity or approved service principals for Azure resources.
- Protect personal data by default. Minimize collection, transmission, storage, logging, and test fixture exposure.
- Use synthetic or anonymized data in tests and docs.
- Design secure failure modes: deny by default, avoid partial auth bypass, and degrade safely.
- Address Wiz, dependency scanning, SAST/DAST, SBOM, and patch findings according to severity and platform SLAs.

Good:

```csharp
if (!authorizationResult.Succeeded)
{
    logger.LogWarning(
        "Refund authorization failed for {OrderReference} and {CorrelationId}",
        command.OrderReference,
        AsyncContext.Current.CorrelationId);

    return ResponseFactory.Forbidden<RefundResponse>();
}
```

Avoid:

```csharp
if (authorizationService.IsUnavailable)
{
    // Never allow sensitive operations because an auth dependency failed.
    return await refundGateway.CreateRefundAsync(request, token, cancellationToken);
}
```

## Data And Persistence

- Implement the data principles in `docs/principles/data-principles.md`.
- Keep each microservice responsible for its bounded context and data store.
- Do not bypass another service boundary by reading or writing its database directly.
- Keep repository/data access code below the application layer.
- Make writes idempotent where retries, duplicate events, or client retries are possible.
- Keep identifiers, timestamps, and status transitions explicit.
- Store only the data the service owns and needs.
- Apply retention, deletion, and right-to-erasure requirements for customer data.

Good:

```csharp
public sealed class RefundRepository(RefundDbContext dbContext) : IRefundRepository
{
    public Task<RefundRecord?> GetByRefundReferenceAsync(
        string refundReference,
        CancellationToken cancellationToken)
    {
        return dbContext.Refunds
            .SingleOrDefaultAsync(refund => refund.RefundReference == refundReference, cancellationToken);
    }
}
```

Avoid:

```csharp
// Do not query another bounded context database directly.
return customerProfileDbContext.Customers
    .Where(customer => customer.Bookings.Any(booking => booking.Reference == orderReference))
    .ToListAsync();
```

## Messaging And Events

- Implement the integration principles in `docs/principles/integration-principles.md`.
- Use Service Bus for asynchronous microservice communication.
- Use CloudEvents or the established EAPI event envelope where the service already follows that pattern.
- Include correlation, causation, operation, session, source, event type, and version metadata where available.
- Message handlers must be idempotent.
- Persist enough state to recover, retry, or roll back when the workflow requires guaranteed delivery or compensation.
- Do not encode business-critical decisions only in transient message headers.
- Version events when consumers depend on their contract.

Example CloudEvent shape:

```csharp
var cloudEvent = new CloudEvent
{
    Id = Guid.NewGuid().ToString(),
    Source = Constants.Application.ServiceBusSource,
    Type = "refund.created.v1",
    DataContentType = "application/json",
    Data = new RefundCreatedEvent
    {
        RefundReference = refund.RefundReference,
        OrderReference = refund.OrderReference,
        Status = refund.Status
    }
};

AsyncContext.Current.MapToCloudEvent(cloudEvent);
```

Example idempotent handler shape:

```csharp
if (await processedEventStore.ExistsAsync(cloudEvent.Id, cancellationToken))
{
    logger.LogInformation("Skipping duplicate event {EventId}", cloudEvent.Id);
    return;
}

await refundEventHandler.HandleAsync(cloudEvent, cancellationToken);
await processedEventStore.MarkProcessedAsync(cloudEvent.Id, cancellationToken);
```

## Infrastructure And Configuration

- Implement the infrastructure principles in `docs/principles/infrastructure-principles.md`.
- Keep environment-specific values in configuration, not code.
- Centralize configuration key names in constants.
- Use private endpoints, managed identity, APIM boundaries, and least-privilege access where applicable.
- Add or update health checks for new downstream dependencies, Service Bus namespaces/topics, data stores, and critical platform services.
- Keep infrastructure, deployment, and pipeline changes in the relevant `VAA.EAPI.Deployments` or `VAA.EAPI.Pipelines*` area.
- Avoid manual-only setup steps. Document any remaining manual steps in the service README.

Good:

```csharp
public static class Environment
{
    public const string RefundApiBaseUrl = "RefundApiBaseUrl";
    public const string RefundServiceBusConnection = "RefundServiceBusConnection";
    public const string RefundTopicName = "RefundTopicName";
}

var refundApiBaseUrl = ConfigReader.Get(Environment.RefundApiBaseUrl);
```

Avoid:

```csharp
var refundApiBaseUrl = "https://prod-refunds.example.internal";
var connectionString = "<service-bus-connection-string>";
```

## Testing

- Add or update focused tests for changed behavior.
- New test projects must use xUnit and coverlet collector.
- Do not introduce Moq for new or amended tests. Prefer real DI registrations, existing fixtures, and `.mock`-driven HTTP/resource mocking through the shared test harness.
- When `.mock`-driven HTTP/resource mocking is not applicable, prefer small hand-written fakes or stubs over Moq.
- Test implemented production types through their public behavior using the mocked resources and fixtures already supported by the owning Core xUnit project. Inspect those capabilities before introducing a new test double or using reflection.
- When the required resource support is unavailable, implement reusable test support in the owning Core xUnit project or an appropriate shared test-support project so both current and future tests can use it. Prefer extending existing support over creating a one-off workaround in an individual test.
- Reflection is a last resort in tests, only when neither the existing mocked resources nor new reusable test support can provide a viable way to exercise the behavior. Do not use reflection to invoke private members or mutate private state when the implemented type can be tested through its public behavior. Document why the alternatives are unavailable wherever reflection remains necessary.
- For HTTP-backed handlers and services, use the real handler/service with `IHttpConnector`, `HttpClientFactoryFixture`, and checked-in `.mock` files for downstream responses; do not replace the HTTP-backed dependency with a fake, stub, or Moq setup when the mock-file harness can cover the behavior.
- If an existing test project only referenced Moq for the test being changed, remove the Moq package reference after replacing the usage. If other legacy tests still use Moq, do not broaden the refactor unless it is in scope.
- Include at least one happy path and one failure or edge case for new handlers.
- Test handlers and services directly; avoid testing business logic only through HTTP triggers.
- Use synthetic test payloads and keep required mock files under an established `.mocks` or test data folder.
- Copy required mock files to output where the test project needs them.
- Do not call live downstream systems from unit tests.
- Use solution-scoped test commands and report any tests that could not be run.
- For HTTP-backed tests, register `IHttpClientFactory` as `VAA.EAPI.Core.Xunit.Http.HttpClientFactory` (microservices) or `VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory` (GraphQL) and register the connector through DI.
- The test-standards harness enforces this for changed HTTP-backed tests by rejecting ad hoc `IHttpConnector` implementations/instance registrations and requiring a checked-in or linked `.mock` resource in the owning test project.
- Configure `ConfigReader.Set(...)` values before building the service provider when the dependency registration or constructor path reads configuration.
- For shared HTTP mocking, use `MockDelegatingHandler:Enabled` and `MockDelegatingHandler:MockServiceUri`; do not hardcode mock endpoints in feature code.
- Keep `.mock` request mapping rules explicit (method + URI + headers + content rules) and deterministic enough to avoid accidental collisions.
- Treat `410 Gone` from the xUnit terminating handler as the expected signal that an external call was not mocked.
- Every non-quick unit-test run must write TRX into a new run-scoped `TestResults/<run-name>` directory and immediately run `scripts/analyse-test-run-results.ps1` against only that run.
- Use `LogFileName` for one test project and `LogFilePrefix` for solution or multi-project runs. Never share a single TRX filename across parallel test projects.
- Full unit-test validation outside `VAA.EAPI.Automation.sln`, including the full `scripts/verify.ps1` path, must collect `XPlat Code Coverage`; report the analyser's coverage table or explicitly state why coverage/analyser output was unavailable.
- A user-requested raw or quick run may omit TRX analysis only when the response explicitly says that no TRX analysis was run.

Good:

```csharp
_serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IHttpClientFactory, VAA.EAPI.Core.Xunit.Http.HttpClientFactory>()
    .AddSingleton<IHttpConnector, HttpConnector>()
    .AddSingleton<IMyService, MyService>()
    .BuildServiceProvider();
```

Good `.mock`-driven HTTP test:

```csharp
public sealed class HttpConnectorMockTests
{
    [Fact]
    public async Task GetAsync_ReturnsMockedResponse_WhenMatchingMockFileExists()
    {
        // Arrange: include a matching .mocks/*.mock file in test output for https://tempuri.org/mock.
        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, VAA.EAPI.Core.Xunit.Http.HttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .BuildServiceProvider();

        var httpConnector = serviceProvider.GetRequiredService<IHttpConnector>();
        var response = await httpConnector.GetAsync("https://tempuri.org/mock", null, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

Avoid:

```csharp
[Fact]
public async Task CreateRefund_CallsLiveApi()
{
    var client = new HttpClient();
    var response = await client.PostAsync("https://prod-refunds.example.internal/refunds", content);

    Assert.True(response.IsSuccessStatusCode);
}
```

## Documentation

- For Mermaid sequence diagrams, follow the authoring rules in `docs/sequence-diagrams.md`: one statement per line, no raw semicolons in labels/messages/notes, and use commas, `<br/>` or `#59;` instead. Run `scripts/check-mermaid-diagrams.ps1 -Path <repository-relative-markdown>` before delivery. The dependency-free `-LintOnly` check catches unsafe punctuation but cannot replace successful parser/render validation; explicitly report unavailable rendering.

- Update the owning service `README.md` when a change affects local setup, configuration, endpoints, triggers, dependencies, or operations.
- When changing an API request or response contract, update the corresponding Bruno collection in the same change. Keep direct-service examples, GraphQL selection sets, scripts, and representative variables aligned with the implemented contract. When a change spans a microservice and GraphQL, update both owning Bruno collections where applicable.
- Update `docs/principles` or `docs/standards` when a change introduces a new durable platform rule.
- When behavior affects a documented sequence flow, update `docs/sequence-diagrams.md`, the owning file under `docs/sequence-diagrams/`, and the matching `docs/behavior-harness.md` sensor entry in the same PR.
- Keep docs clear enough for future engineers and AI agents to apply without hidden context.
- Do not include secrets, real customer data, sensitive operational data, or internal credentials in documentation.

## Pull Request Notes

PR descriptions should include:

- A short summary of the change.
- Impacted services, projects, or platform areas.
- Tests and verification commands run.
- A short note explaining skipped tests or residual risk when verification could not be run.

## AI Agent Checklist

Before finishing a code change, an AI agent must confirm:

- The change is scoped to the owning solution and bounded context.
- New or changed application code follows the relevant principle documents.
- Triggers, resolvers, handlers, services, and infrastructure clients keep their responsibilities separate.
- Outbound HTTP uses `IHttpConnector` unless an approved infrastructure exception applies.
- Configuration keys are centralized and secrets are not committed.
- Regex expressions use the owning platform's shared `RegexCompiled` helper unless a documented exception applies.
- Errors, logs, telemetry, and response contracts are consistent with the owning service.
- Sensitive data is minimized and not logged.
- Tests were added or updated for meaningful behavior, or the reason for not testing is documented.
- The relevant solution-scoped build/test command was run where feasible.
- Owning Bruno collections were updated for any changed API request or response contract.
- Service README, deployment, pipeline, principle, or standard docs were updated where needed.
