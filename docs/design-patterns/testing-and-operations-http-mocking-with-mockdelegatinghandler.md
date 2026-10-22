# HTTP Mocking With MockDelegatingHandler

**Pattern group:** Testing And Operations

**Status:** Recommended

**Last reviewed:** 2026-06-25

**Owner:** EAPI Technical Leads

## Intent

Use the shared HTTP mocking stack (`MockDelegatingHandler`, `.mock` files, and the Automation HTTP mock service) to test and simulate downstream behavior without live dependencies.

This keeps handler and service tests deterministic while preserving the same connector and HTTP pipeline behavior used at runtime.

## Use When

- Simulating downstream HTTP responses in DEV, SIT, performance, or test harness contexts.
- Testing handler or service behavior through `IHttpConnector`.
- Validating request-to-mock matching with method, URI, headers, and payload rules.
- Returning dynamic response content (GUIDs, dates, random values) for test scenarios.

## Do Not Use When

- The test needs a true contract/integration check against a live partner environment.
- Production runtime behavior would depend on mocking.

## Pattern Shape

1. Route outbound calls through `IHttpConnector` and shared `HttpClientFactory`.
2. Enable mock mode using `MockDelegatingHandler:Enabled` and `MockDelegatingHandler:MockServiceUri`, represented in the owning configuration provider's supported shape.
3. Define one or more `.mock` files with `###Request` and `###Response` sections.
4. Match by request method + URI, then headers, then optional content rules.
5. Return static or dynamic response content.
6. If no mock matches: runtime falls back to the real downstream call; xUnit tests should return `410 Gone` from the terminating handler.

## Environment Mock Service

- Mock service endpoint pattern:
  - `https://testresources.virginatlantic.com/<dev|sit>/mock/<url-encoded-uri>`
- Supported request methods in the mock function include:
  - `GET`, `PUT`, `POST`, `PATCH`, `DELETE`, `OPTIONS`, `HEAD`
- If no mock is mapped, the service returns `501 Not Implemented`.
- The request URI appended to the environment endpoint must be URL encoded.

## Mock File Format

- Files use the `.mock` extension.
- Files must contain exactly one `###Request` section and one `###Response` section.
- Lines beginning with `--` are treated as comments and ignored by the parser.
- Request and response follow raw HTTP message shape.

Example:

```http
###Request
GET https://tempuri.org/mock HTTP/1.1
accept: application/json

###Response
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Response-Delay: 00:00:00

{ "message": "Welcome to a mocked response" }
```

## Request Mapping Rules

Mocks are selected in this order:

1. Method + URI + protocol line match.
2. Header checks from the mock request section.
3. Optional content rules (newline-separated after the blank line in request content).

Matching details:

- Header values support exact or regex-style matching.
- For `application/json`, each rule is treated as a JSONPath/JPath expression and must select at least one token.
- For `application/xml`, each rule is treated as XPath and must match a node.
- For other media types, each rule is evaluated as regex.
- All configured rules must evaluate true for the mock to match.

## Dynamic Data Commands

Dynamic placeholders are supported in mock response bodies:

- `{Guid()}`
- `{DateTime(format)}`
- `{DateTime(format, offset)}`
- `{DateTime(format, offset, offsetType)}`
- `{RandomNumber(max)}`
- `{RandomNumber(min, max)}`
- `{RandomDateTime()}`
- `{RandomString()}`
- `{RandomString(length)}`
- `{RandomString(length, format)}`
- `{RandomItem(arg1, arg2, ...)}`
- `{Loop(n)} ... {Index()} ... {Loop}`

Notes:

- `offsetType` supports `millisecond`, `second`, `minute`, `hour`, `day`, `week`, `month`, `year`.
- Loop iteration counts down toward `0`.

For fixtures subject to expiry or retrieval windows, use relative `DateTime` commands rather than moving hardcoded dates into a later year. Move related itinerary dates together, preserving durations and times of day. For example, an 18-day inclusive holiday can depart at `{DateTime(yyyy-MM-ddT00:00:00, 30, day)}` and return at `{DateTime(yyyy-MM-ddT00:00:00, 47, day)}`.

Keep assertions on required dates and times; do not skip them when a value is null. The current parser evaluates `DateTime` using local `DateTime.Now`, separately for each command. Date-relative assertions should bracket response evaluation to tolerate midnight rollover. Keep happy-path fixtures comfortably inside their valid window; use a controlled clock for exact expiry-boundary tests, especially where production compares UTC. Retain explicitly expired scenarios rather than making every fixture future-dated.

## Azure Function Integration

For services using shared HTTP infrastructure, enable mock routing through configuration:

```json
{
  "MockDelegatingHandler": {
    "Enabled": true,
    "MockServiceUri": "https://testresources.virginatlantic.com/dev/mock"
  }
}
```

Guidance:

- Enable by environment (typically DEV/SIT).
- Keep disabled for UAT/production unless there is an explicit, approved exception.
- Do not hardcode mock URLs in feature code.

## Mock Storage And Synchronisation

Automation mock files are stored in the configured mock storage location (local folder, or the Azure Blob container configured via `HttpMocker.AzureBlobContainerService.Container`) and are loaded into memory by the mock service.

The shared DEV/SIT service stores `*.mock` files in its `mocks` blob container and synchronises storage into the running function approximately every 20 minutes. Use the manual endpoints when a scenario cannot wait for the scheduled refresh.

Manual sync endpoints:

- `GET /<env>/mock/synchronise` to reload all mocks.
- `GET /<env>/mock/synchronisemock/{*mockName}` to reload one named mock.

## xUnit Test Integration

For unit tests using `VAA.EAPI.Core.Xunit.Http.HttpClientFactory`:

- Place `.mock` files under a `.mocks` folder in the test project.
- Ensure `.mocks` files are copied to test output if required by the project.
- Register the shared xUnit `IHttpClientFactory`, the real `IHttpConnector`, and the real unit under test through DI. Configure any required `ConfigReader` values before building the provider.
- Call the unit's public method and assert its returned behavior. Use `HttpClientFactory.GetResponses(uri)` to inspect the matched outbound request and assert its URI, headers, or body when those are part of the behavior under test.
- Match the URI in the test call to the method and URI in the mock's `###Request` line. A different URI will not select that mock.
- Unmatched external calls return `410 Gone` from the terminating handler to enforce no-live-network rules.

For example, the checked-in `.mocks/soap_tempuri.mock` fixture matches `GET https://tempuri.org/mock` and returns a page containing `Welcome to a mocked page`. A test using that fixture can exercise the real connector and inspect the request:

```csharp
AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };

using var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddSingleton<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>()
    .AddSingleton<IHttpClientFactory>(services =>
        services.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>())
    .AddSingleton<IHttpConnector, HttpConnector>()
    .BuildServiceProvider();

var connector = serviceProvider.GetRequiredService<IHttpConnector>();
var response = await connector.GetAsync("https://tempuri.org/mock", null, null);

Assert.Equal(HttpStatusCode.OK, response.StatusCode);
Assert.Contains("Welcome to a mocked page", response.ReadResponseAsString());

var factory = serviceProvider.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>();
var recorded = Assert.Single(factory.GetResponses("https://tempuri.org/mock"));
Assert.Equal("https://tempuri.org/mock", recorded.RequestMessage?.RequestUri?.ToString());
```

For a handler or service test, register that production class in the same provider, call its public method, and assert both its result and the relevant recorded downstream request. The `.mock` fixture supplies the external HTTP response; it does not replace the unit under test.

## Other Mocked External Resources

In the microservices automation stack, mocked support exists (or is being introduced) beyond HTTP for:

- Table Storage
- Blob Storage
- Service Bus
- Distributed Cache
- Message Dispatcher
- Cosmos (in-progress)

These resource fixtures are currently a microservices capability. Cosmos support is not yet a completed baseline. Use the owning solution's existing fixtures/helpers rather than introducing new ad hoc mocking frameworks.

## Source Wiki Pages

This document ports the maintained engineering content from the internal Azure DevOps pages for [API Mocking](https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_wiki/wikis/Enterprise-API-Management-Team.wiki/20900/API-Mocking), [Request mapping](https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_wiki/wikis/Enterprise-API-Management-Team.wiki/20909/Request-mapping), [Dynamic data](https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_wiki/wikis/Enterprise-API-Management-Team.wiki/20907/Dynamic-data), [Example mock file](https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_wiki/wikis/Enterprise-API-Management-Team.wiki/20905/Example-mock-file), [Azure function integration](https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_wiki/wikis/Enterprise-API-Management-Team.wiki/20904/Azure-function-integration), [xUnit test integration](https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_wiki/wikis/Enterprise-API-Management-Team.wiki/21218/xUnit-test-integration), and [Other mocked external resources](https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_wiki/wikis/Enterprise-API-Management-Team.wiki/24481/Other-mocked-external-resources). Repository guidance wins if a wiki page later conflicts with executable code or harness policy; reconcile the drift in both places.

## Code References

- [MockDelegatingHandler.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Http/MockDelegatingHandler.cs:13): runtime mock routing and `501` fallback handling.
- [HttpMock.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Http/Mocking/HttpMock.cs:13): `.mock` parser, markers, and raw HTTP shape.
- [HttpMockExtensions.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Extensions/HttpMockExtensions.cs:17): method/URI/header/content rule matching.
- [CommandParser.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Http/Mocking/CommandParser.cs:12): dynamic command parsing.
- [MockFunction.cs](../../VAA.EAPI.Automation/VAA.EAPI.Automation.Utility.HttpMocker/MockFunction.cs:13): mock service endpoints and sync operations.
- [HttpClientFactory.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Core.Xunit/Http/HttpClientFactory.cs:11): xUnit HTTP pipeline with mocking.
- [TerminatingDelegatingHandler.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Core.Xunit/Http/TerminatingDelegatingHandler.cs:1): `410 Gone` enforcement for external calls in unit tests.

## Related Principles And Standards

- [Coding Standards](../standards/coding-standards.md)
- [Integration Principles](../principles/integration-principles.md)
- [Security Principles](../principles/security-principles.md)
- [EAPI Function Test Harness](testing-and-operations-eapi-function-test-harness.md)

## AI Agent Checklist

- Is mock usage scoped to test/DEV/SIT and not production behavior?
- Are `.mock` request/response sections valid and deterministic?
- Are content rules specific enough to avoid accidental match collisions?
- Are tests still blocking live outbound calls?
- Are mock examples and payloads synthetic and safe?

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-25 | Added consolidated HTTP mocking guidance from API Mocking, request mapping, dynamic data, Azure function integration, xUnit integration, and external resource notes. | Dimitris Dimitriou |
| 2026-06-25 | Refined guidance to remove hand-written fake preference and align xUnit setup on `VAA.EAPI.Core.Xunit.Http.HttpClientFactory` registration. | Dimitris Dimitriou |
