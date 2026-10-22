# API Automation Patterns (VAA.EAPI.Automation)

## Locate Existing Helpers First

- Search within the target domain (e.g., `VAA.EAPI.Automation.MyAccount`) for `Requests/Endpoint` helpers before creating new HTTP clients.
- Use existing helper methods to build requests and keep tests focused on inputs/outputs.

## Typical Test Structure

- Arrange inputs using shared helpers (`DataGenerator`, `Config.Environment`, fixtures).
- Call a helper method from a GraphQL/microservice wrapper.
- Assert payload structure and error codes in-place.

Example pattern from GraphQL tests:

```csharp
var emailOtpResponse = GraphQL.GenerateEmailOtp(env, emailId).EnsureIsSuccessStatusCode();
JObject emailOtpResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await emailOtpResponse.Content.ReadAsStringAsync());
Assert.True(emailOtpResult["data"]["accountOTPGenerate"] != null);
```

See the MyAccount GraphQL tests for full examples of this pattern.

## Response Handling Conventions

- Prefer `EnsureIsSuccessStatusCode()` or `EnsureGQLResponseSuccess()` to fail fast on non-OK responses.
- Deserialize JSON using `JsonConvert` + `JObject` for flexible payload assertions.
- Keep assertions descriptive; include the full response in failure messages when possible.

## Data Setup Patterns

- Reuse helper classes in `Helpers` or `DataProvider` for test data setup (e.g., storage table lookups).
- Keep setup logic in helpers rather than in the test body once it repeats.
- When a helper returns a token or entity, pass that into the request helper; avoid recomputing it per test step.

## When Adding New Helpers

- Place new helper classes under `Helpers/` or `Requests/Endpoint` to match existing organization.
- Expose small, focused methods that return responses or parsed results; let tests handle assertions.
- Keep configuration sources consistent with existing helpers (e.g., `TestConfig`, `Config.Environment`).

## JSON-Driven E2E Test Pattern

The GraphQL E2E booking suite uses a data-driven pattern where test scenarios are defined in JSON with schema validation:

```
EndtoEnd/
├── e2e-scenarios.json          ← QAs edit this to add/modify scenarios
├── e2e-scenario-schema.json    ← Provides intellisense and validation
├── E2EScenario.cs              ← Scenario config class (IXunitSerializable)
├── E2ETestData.cs              ← Loads JSON, provides filtered MemberData
├── E2ETestHelper.cs            ← Flow step helpers
└── E2EGraphQL.cs               ← Test class (thin orchestration)
```

Key principles:
- Test scenarios are JSON objects with properties like `origin`, `destination`, `bookingType`, `seatMode`, `insurance`
- The JSON Schema enforces conditional required fields (e.g. `seatProductName` required when `seatMode != "None"`)
- `E2ETestHelper` contains one method per flow step; conditional logic lives in the helper, not the test
- Filtered `[MemberData]` methods (`SanityScenarios`, `RegressionScenarios`) enable trait-based pipeline filtering
- `E2EScenario` implements `IXunitSerializable` so each scenario appears individually in Test Explorer

## Composition Pattern for Multi-Target Tests

When the same test flow needs to write to different storage targets (e.g. backend vs mobile app):

```csharp
// Shared flow helper (no storage writes)
public class OciRevenueFlowHelper
{
    public async Task RunRevenuePNRCreationForOneway(...) { /* booking flow */ }
}

// Backend test class — own traits, writes to OCIPNRs
public class RevenuePNRCreationTest
{
    [Trait("Category", "Oci-Pnr-Creation")]
    public async Task RevenuePNRCreationForOneway(...)
    {
        await _flow.RunRevenuePNRCreationForOneway(...);
        await WriteToAzureStorage.AddOCIPNRToAzureStorageTable(_testConfig);
    }
}

// Mobile app test class — own traits, writes to OnlineCheckInMobileAppData
public class MobileAppRevenuePNRCreation
{
    [Trait("Category", "MobileApp-OciPnrCreation")]
    public async Task RevenuePNRCreationForOneway(...)
    {
        await _flow.RunRevenuePNRCreationForOneway(...);
        await WriteToAzureStorage.AddOciMobileAppPnrsToStorageTable(_testConfig);
    }
}
```

This avoids trait leakage (where inherited traits cause tests to run in unintended pipelines) while keeping the booking flow in one place.
