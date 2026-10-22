# Test Data Strategy (VAA.EAPI.Automation)

This guide outlines how to manage test data for API/UI/mobile automation suites.

## Principles

- Prefer deterministic, reusable data over brittle ad-hoc records.
- Keep data setup logic in shared helpers/providers when reused.
- Minimize cross-test dependencies and mutable shared state.

## Base URLs

The solution exposes two near-identical-looking base-URL config classes in
[`VAA.EAPI.Automation.Core/Config.cs`](../VAA.EAPI.Automation.Core/Config.cs)
that are NOT interchangeable. Picking the wrong one is an easy mistake — a
test can "pass" against the wrong host if both happen to expose the path
locally, then silently break later.

| Config class | Reads | Host in test | Host in prod | When to use |
| --- | --- | --- | --- | --- |
| `Config.TestResourcesVirginAtlantic` | `BaseUrl` | `testresources.virginatlantic.com` | `resources.virginatlantic.com` | **Default.** Customer-facing public APIs. ~367 call sites across the solution. |
| `Config.DigitalTestVirginAtlantic` | `BaseUrlDigital` | `digital-test.virginatlantic.com` | *(no equivalent — fall back to `TestResources`)* | Internal-only paths: `/digital/vred/*` (Flying Club internals), legacy raw `/digital/graphql`. ~26 call sites, all niche. |

**Rule of thumb:** if you're writing a new test, you almost certainly want
`Config.TestResourcesVirginAtlantic`. The Gateway REST shim family
(`/digital/Graphql/rest/v1/booking`, `…/accountMemberDetails`, etc.) lives
under `testresources.virginatlantic.com` — the `digital/...` part is in the
path, not the host. Don't be fooled by the word "Digital" in the config-class
name.

Reach for `Config.DigitalTestVirginAtlantic` only if both are true:
1. You're hitting an internal `/digital/vred/...` or raw `/digital/graphql` URL.
2. The path doesn't exist on `testresources.virginatlantic.com` for that env.

When you do reach for it, **add a `_env.Is("prod")` fallback to
`TestResourcesVirginAtlantic`** — see
[`Core/Helpers/Requests/Endpoints.cs:73`](../VAA.EAPI.Automation.Core/Helpers/Requests/Endpoints.cs#L73)
for the established pattern. The internal host doesn't exist in prod.

## Default: storage-table-driven data

**Storage-table-driven data is the default for every new test that requires PNRs,
loyalty accounts, holiday bookings, or any other booking-shaped record.** Do not
hard-code PNRs, names, or DOBs in test classes — the values rot the moment the
backing booking is cancelled, expires, or is consumed by another suite.

Use `VAA.EAPI.Automation.Core.Helpers.StorageAccount.RetrieveFromStorageTable`
(in `VAA.EAPI.Automation.Core`) as the single entry point. It exposes filtered
lookups against the shared Azure Tables used by the team's PNR-creation pipeline.

Common helpers (see the source for the full list and filter parameters):

| Helper | Table | Use for |
| --- | --- | --- |
| `RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyType, reservationType, …)` | `MobileAppPnrs` / `PNRs` | Generic MyTrips-style PNRs created this week |
| `RetrievePNRDetailsFromStorageTableForSpecificRoute(journey, origin, dest, …)` | `MobileAppPnrs` | Route-pinned scenarios |
| `RetrievePNRDetailsFromStorageTableForSeatScenarios(…)` | `MobileAppPnrsForSeatScenarios` | Seat-flow tests |
| `RetrieveOCIPNRDetailsFromStorageTable(…)` | `OCIPNRs` | Online check-in (within the 1h 20m – 24h window) |
| `RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tier, points, …)` | `LoyaltyAccounts` | Flying Club member fixtures |
| `RetrieveHolidayBookingDetailsFromStorageTable(…)` | `vHolsBookingsTestE` / `…F` | Virgin Holidays bookings |
| `RetrievePassportDetailsFromStorageTable(countryCode)` | `PassportTestingData` | Passport numbers for OCI |

Each helper returns a `Dictionary<string, string>` keyed by the contract documented
at the call site (`"pnr"`, `"firstName"`, `"lastName"`, `"passengersCount"`, …).
When no row matches the filters, the dictionary contains a single `"error"` entry
— surface it as a test failure rather than letting it null-deref downstream.

### Canonical pattern

```csharp
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

[Fact]
public async Task TC01_Booking_ReturnsSuccess()
{
    // Pull a fresh PNR from the shared storage table — no hard-coded values.
    Dictionary<string, string> pnrDetails = RetrieveFromStorageTable
        .RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "REVENUE");

    if (pnrDetails.TryGetValue("error", out string? error))
    {
        Assert.Fail($"Test case failed: {error}");
    }

    string pnr        = pnrDetails["pnr"];
    string givenNames = pnrDetails["firstName"];
    string surname    = pnrDetails["lastName"];

    // Hand the fetched values to the endpoint wrapper. REST endpoint here, so
    // EnsureIsSuccessStatusCode is enough — for GraphQL use EnsureGQLResponseSuccess
    // (see § Response assertions).
    HttpResponseMessage response = GatewayEndpoint.Booking(
        env,
        usingString: "PNR",
        confirmationNum: pnr,
        givenNames: givenNames,
        surname: surname
    ).EnsureIsSuccessStatusCode();

    string body = await response.Content.ReadAsStringAsync();
    Assert.False(string.IsNullOrWhiteSpace(body));
}
```

> Note the example uses **explicit types** (`Dictionary<…>`, `HttpResponseMessage`,
> `string`) rather than `var`, and **single-line `//` comments** instead of
> `#region Test Data` / `#region API Call` blocks. Both are codified in
> [`AGENTS.md` § Design conventions](../AGENTS.md#design-conventions).

## Response assertions

GraphQL endpoints will return **HTTP 200 even when the operation failed** —
the failure is reported as an `errors` array inside the response body. A plain
`EnsureIsSuccessStatusCode()` will happily pass on those responses and the test
only fails much later (or, worse, silently asserts against an empty `data`
section).

Pick the assertion to match what the endpoint actually is — not what the URL
path looks like. Some REST endpoints live under `/Graphql/…` paths but return
RFC-style status codes; some endpoints with vanilla-looking URLs are GraphQL
underneath. If in doubt, check the response body shape (presence of
`data`/`errors` keys) or ask the owning team.

| Endpoint behaviour | Use |
| --- | --- |
| GraphQL — returns 200 with `data` / `errors` body | `EnsureGQLResponseSuccess()` |
| REST — failure reflected in HTTP status | `EnsureIsSuccessStatusCode()` |
| A specific non-2xx is the success case | `EnsureStatusCodeIs(HttpStatusCode.XYZ)` |

`EnsureGQLResponseSuccess` (in
[`VAA.EAPI.Automation.Core.Extensions.HttpResponseMessageExtensions`](../VAA.EAPI.Automation.Core/Extensions/HttpResponseMessageExtensions.cs))
runs `AssertIsSuccessStatusCode` *and* `AssertIsGQLSuccess` — the latter parses
the body and fails the test if an `errors` node is present. That second check
is the whole point: a GraphQL endpoint will quite happily return HTTP 200 with
an `errors` array describing the failure, and `EnsureIsSuccessStatusCode` on
its own will pass that false-positive straight through.

Examples in the codebase:

- GraphQL: `VAA.EAPI.Automation.MyAccount/DataProvider/LoyaltyHouseHoldAccountCreation.cs`
  — `GraphQL.GenerateEmailOtp(...).EnsureGQLResponseSuccess()`.
- REST: `VAA.EAPI.Automation.Gateway/Tests/Rest/BookingTests.cs` (above) —
  the `/digital/Graphql/rest/v1/booking` route uses status codes, so
  `EnsureIsSuccessStatusCode()` is correct despite the `/Graphql/` segment in
  the URL.

For decision-style flows where you need a boolean rather than an assertion, use
`response.IsGQLResponseSuccess()` — same logic, returns `bool` instead of
throwing.

### Body assertions

Status + (where applicable) `errors`-node checks tell you the *call* worked.
They don't tell you the response actually describes what you asked for. For any
test whose intent is "did the booking I requested come back correctly", add
body-level assertions on the parsed payload — the
[Trips microservice tests](../VAA.EAPI.Automation.Trips/Tests/TripsTests/Microservices/TripsTests.cs)
are the reference pattern. The Gateway booking test
([`BookingTests.cs`](../VAA.EAPI.Automation.Gateway/Tests/Rest/BookingTests.cs))
follows it for the same booking-lookup response shape.

Standard ladder:

1. **Non-empty body** — fail fast if the body is blank, before deserialising.
2. **Body-level error guards** — known error markers the endpoint returns
   *inside* a 200 response (e.g. `"NGTMYTR400"`,
   `"Unable to retrieve reservation"`). A body scan with `Contains(...)` is
   defensible here because these markers come from a fixed error catalogue.
3. **Echo-back checks** — fields you supplied in the request that the response
   must reflect (`pnrLocator` matches the PNR you asked for; passenger names
   match where they're part of the request envelope). These catch
   wrong-record bugs that status codes never will.
4. **Shape invariants tied to the test scenario** — e.g. OneWay → 1 slice,
   Return → 2 slices, "multi-PNR list" → `tripListResponse.Count == requested`.
5. **Field presence / value checks** for anything else the scenario hinges on.

```csharp
string responseBody = await response.Content.ReadAsStringAsync();
Assert.False(string.IsNullOrWhiteSpace(responseBody), "Booking response body was empty");
JObject bookingResponse = JObject.Parse(responseBody);

string bodyString = bookingResponse.ToString();
Assert.False(bodyString.Contains("NGTMYTR400"),
    $"Unexpected error: NGTMYTR400. Body: {bodyString}");

string actualPnrLocator = bookingResponse
    .RequireToken("product[0].reference.pnrLocator")
    .ToString();
Assert.True(pnr.Is(actualPnrLocator),
    $"PNR locator mismatch. Expected: {pnr} Actual: {actualPnrLocator}");

JToken slicesArray = bookingResponse.RequireToken("product[0].slices");
Assert.True(slicesArray.Count() == 1, $"OneWay journey should have exactly 1 slice but found {slicesArray.Count()}");
```

> The paths above (`product[0].…`, no `data` or `Booking` wrapper) are specific
> to the Gateway REST shim. The underlying [Trips GraphQL endpoint](../VAA.EAPI.Automation.Trips/Tests/TripsTests/GraphQL/TripsTests.cs)
> wraps the same payload in `data.Booking.…`, and the
> [Trips microservice](../VAA.EAPI.Automation.Trips/Tests/TripsTests/Microservices/TripsTests.cs)
> uses a lowercase `booking.…` root. JSONPath is case-sensitive — confirm the
> shape against the real response before writing paths.

#### Navigating response JSON safely

GraphQL/REST bodies are deeply nested and every indexer in Newtonsoft's
`JObject`/`JToken` returns a *nullable* token. Chaining indexers (each one a
`?.[…]`) emits CS8602 warnings the whole way and throws `NullReferenceException`
at the first null indexer — with no indication of *which* segment was missing.
Both problems are avoidable.

**Default for tests: `RequireToken(...)`.** Use the
[`RequireToken` extension](../VAA.EAPI.Automation.Core/Extensions/JObjectExtensions.cs)
on `Core.Extensions.JObjectExtensions`. It takes a JSONPath string and either
returns a non-null `JToken` or throws a `JsonException` naming the failing
path:

```csharp
string actualPnrLocator = bookingResponse
    .RequireToken("product[0].reference.pnrLocator")
    .ToString();
```

If that path is missing or resolves to JSON `null`, the test fails with a
message naming `product[0].reference.pnrLocator` rather than a bare NRE. Don't
write the equivalent indexer chain (`bookingResponse["product"]?[0]?["reference"]?["pnrLocator"]?.ToString()`) — that's the pattern this rule is here to eliminate from new tests.

**Parse with `JObject.Parse`, not `JsonConvert.DeserializeObject<JObject>`.**
`JObject.Parse` throws `JsonReaderException` on malformed JSON and never
returns null, so the result variable is a plain `JObject` — no `JObject?` and
no null-guard noise:

```csharp
JObject bookingResponse = JObject.Parse(responseBody);
```

`JsonConvert.DeserializeObject<JObject>(responseBody)` is the pattern to
avoid: its return type is `JObject?`, which forces a null check every test
author keeps forgetting.

**Don't blanket-suppress with `!`.** A `!` null-forgiving operator hides the
problem without solving it. `RequireToken` removes the need for `!` entirely
on the call site; if you find yourself reaching for `!`, that's a signal to
either add a new typed helper to `JObjectExtensions` or to assert the
intermediate token explicitly with a meaningful message.

**Schema-shape validation** (rather than per-field content) — use
[`Core.Helpers.SchemaValidator.JsonSchemaValidator`](../VAA.EAPI.Automation.Core/Helpers/SchemaValidator.cs)
against a schema pulled from `Endpoints.GetSchema(...)`. See the
`TripsSchemaValidation` theory in the Trips microservice tests for the worked
example.

For a worked multi-PNR example see
[`VAA.EAPI.Automation.Trips/Tests/TripsTests/Microservices/TripsTests.cs`](../VAA.EAPI.Automation.Trips/Tests/TripsTests/Microservices/TripsTests.cs).

### When you genuinely need hard-coded data

The narrow exceptions — and the only times a reviewer should accept a literal in
a test — are:

1. **Negative-path inputs** that must be invalid by construction (e.g. `"PNR-Invalid"`,
   `"INVALID_TOKEN"`). These belong inline; storage tables only hold valid rows.
2. **Schema/contract tests** asserting structural shape against a stub.
3. **Pure unit tests** (`VAA.EAPI.Automation.Core.Tests`) of helper logic.

Anything else — a real confirmation code, a real surname, a real loyalty number —
goes through `RetrieveFromStorageTable`.

## Other data sources (in order of preference)

1. **Storage tables** (above) — preferred default.
2. **Helper-driven dynamic generation** (`Core.Helpers.DataGenerator`,
   `FlyingClubMemberHelper`) — for write-path scenarios needing fresh records.
3. **Environment configuration** (`local.settings.local.json`, variable groups) —
   for endpoint URLs, subscription keys, and connection strings only.
4. **Inline literals** — only under the exceptions listed above.

## Data Patterns

### 1) Storage-table lookup (default)

Filter on the smallest predicate set that still satisfies the scenario, so the
helper returns enough candidates to pick one randomly. Over-constrained filters
turn into intermittent "No matching PNR found" failures.

### 2) Static baseline + dynamic suffix

Use a stable baseline payload and append unique identifiers where collisions are
possible (e.g. write-path tests creating fresh entities).

### 3) Scenario-owned fixtures

Each suite/scenario should own setup/teardown where possible to keep tests
isolated.

### 4) Shared providers

If setup steps repeat across tests, move them to `Helpers`/`DataProvider`
classes in the project — or, if cross-project, into `Core.Helpers`.

## Isolation Guidance

- Avoid reusing mutable entities across unrelated tests.
- Prefer creating fresh records for write-path tests.
- For read-only validations, use known stable fixture records.

## Cleanup Guidance

- If tests create long-lived records, define cleanup behavior (immediate or
  scheduled). See `TestDataCleanUpTests.cs` in the Trips suite for the existing
  pattern.
- Document cleanup ownership in project README or helper comments.

## Secrets & Sensitive Data

- Do not commit credentials, tokens, or production-like personal data.
- Use environment-specific secret stores and local gitignored config files
  (`local.settings.local.json`).
- Storage-table connection strings are pulled from `Vault.Default` (Key Vault)
  inside `RetrieveFromStorageTable` — never hand-roll them in a test.

## Definition of Good Test Data

Test data is "good" when it is:

- Sourced from the shared storage tables (or the documented exceptions above).
- Predictable in assertions.
- Easy to regenerate.
- Isolated from unrelated scenarios.
- Safe for non-production environments.
