# EAPI Function Test Harness

**Pattern group:** Testing And Operations

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Use EAPI test fixtures, fake Function contexts, sample payloads, and connector-compatible HTTP client factories to test handlers, services, and function boundaries without calling live downstream systems.

This pattern makes platform-specific code testable while preserving the same connector and context behavior used at runtime.

## Use When

- Testing handlers or services that rely on `IHttpConnector`.
- Testing code that reads `AsyncContext`, `FunctionContext`, headers, or binding data.
- Loading request and response samples for deterministic unit tests.

## Do Not Use When

- The scenario needs a live contract/integration check instead of a harnessed unit test.
- The test would call live downstream systems.
- The behavior belongs in an integration or contract test environment.

## Pattern Shape

1. Register `IHttpClientFactory` as `VAA.EAPI.Core.Xunit.Http.HttpClientFactory` to provide connector-compatible HTTP behavior.
2. Use `TestFunctionContext` where function context or `AsyncContext` is needed.
3. Use `.mocks` HTTP files (or deterministic JSON/XML samples) from test data folders.
4. Build handlers through DI or explicit constructor injection.
5. Assert EAPI response status, values, and errors.

## Implementation Guidance

- Prefer handler and service tests over trigger-only tests for business behavior.
- Exercise implemented production types through public behavior with the owning Core xUnit project's supported mocked resources and fixtures before considering reflection or new test doubles.
- If the required resource capability is missing, add reusable support to the owning Core xUnit project or an appropriate shared test-support project for current and future tests. Avoid one-off workarounds that bypass the implemented type.
- Use reflection only as a last resort when existing fixtures and new reusable support cannot provide a viable alternative; document why those alternatives are unavailable. Follow the [testing standards](../standards/coding-standards.md#testing).
- Use synthetic or anonymized test data.
- Keep `.mocks` and mock payload files copied to output where required.
- Avoid live network calls in unit tests.
- Initialise `AsyncContext.Current` when the code under test expects it.
- Register `IHttpClientFactory` and `IHttpConnector` in tests so handler code executes through the real HTTP pipeline.
- Keep `MockDelegatingHandler:Enabled` and `MockDelegatingHandler:MockServiceUri` configuration explicit in test setup when validating environment mock-service routing.
- Assert unmatched external calls are blocked by the xUnit terminating handler (`410 Gone`) rather than silently calling live systems.

See [HTTP Mocking With MockDelegatingHandler](testing-and-operations-http-mocking-with-mockdelegatinghandler.md) for request mapping, dynamic data commands, and mock file structure.

## Code References

- [HttpClientFactory.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Core.Xunit/Http/HttpClientFactory.cs:11): connector-compatible HTTP test factory for DI registration.
- [TestFunctionContext.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Core.Functions/TestFunctionContext.cs:10): fake Functions context.
- [TestHelpers.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Microservices.Framework.Tests/TestHelpers.cs:16): sample payload and test user helpers.
- [MockDelegatingHandler.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Core.Xunit/Http/MockDelegatingHandler.cs:8): `.mocks` loading and dynamic response parsing.
- [TerminatingDelegatingHandler.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Core.Xunit/Http/TerminatingDelegatingHandler.cs:1): external-call blocking for unit tests.

## Related Principles And Standards

- [Coding Standards](../standards/coding-standards.md)
- [Security Principles](../principles/security-principles.md)
- [Infrastructure Principles](../principles/infrastructure-principles.md)

## AI Agent Checklist

- Tests avoid live downstream systems.
- Test context is initialised when required.
- Existing Core xUnit resource support is reused, or missing support is implemented for reuse by current and future tests; any reflection has a documented lack of viable alternatives.
- Test data is synthetic and safe.
- Both success and failure paths are covered for new behavior.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
| 2026-06-25 | Updated testing guidance to prefer `.mock`-driven TDD resources and DI registration of `IHttpClientFactory` using `VAA.EAPI.Core.Xunit.Http.HttpClientFactory`. | Dimitris Dimitriou |
