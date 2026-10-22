# Integration Principles
**VAA.EAPI.Automation**

---

The suite integrates with EAPI through HTTP/REST, GraphQL, and SOAP endpoints, exercised
via shared request wrappers and assertion helpers. Integrations must go through the
established wrappers so requests are consistent, responses are checked correctly, and a
failure is attributed to the service — not to a bespoke client in a test. AI agents must
reuse the request and response layers rather than hand-rolling HTTP.

## Reuse Request Wrappers

Feature tests call endpoint wrappers, not raw HTTP. This is the test-suite form of the
platform's "use the shared HTTP abstraction, not a hand-rolled client" rule (the platform
[Integration Principles](../../../doc/principles/integration-principles.md) mandate
`IHttpConnector` for product code; here the equivalent shared layer is the suite's request
wrappers).

- Use the existing GraphQL / microservice / endpoint wrappers (`Requests/Endpoints`,
  `Requests/Factory`, gateway wrappers) for the target domain before writing anything new.
- Do not construct `HttpClient`, hold static clients, or assemble requests by hand in a
  test body — request construction, headers, and serialisation belong in the wrapper layer.
- Pass fetched test data (PNR, member number, token) into the wrapper; do not recompute it
  per step.
- If a wrapper is missing a capability, extend the wrapper (under `Requests/…`), keeping
  tests focused on arrange/act/assert.

## Assert The Right Way For The Protocol

The assertion must match what the endpoint *is*, not what its URL looks like.

- **GraphQL:** use `EnsureGQLResponseSuccess()`. GraphQL returns **HTTP 200 with an
  `errors` array** on failure, so a status-only check passes false-positives.
- **REST:** use `EnsureIsSuccessStatusCode()` — including REST routes that happen to live
  under `/Graphql/…` paths.
- Some vanilla-looking URLs are GraphQL and some `/Graphql/…` paths are REST shims; decide
  by the endpoint's real behaviour. See
  [`TEST_DATA_STRATEGY.md` § Response assertions](../TEST_DATA_STRATEGY.md#response-assertions).

## Navigate Responses Safely

Parsing must fail with a useful message, not a bare null-reference.

- Parse with `JObject.Parse(body)` (throws on malformed, returns non-null) and walk paths
  with `response.RequireToken("booking.product[0].reference.pnrLocator")` from
  `Core.Extensions.JObjectExtensions`.
- Do not chain `JsonConvert.DeserializeObject<JObject>(…)["…"]["…"]` indexers (CS8602
  warnings, bare NRE on failure with no path context), and do not suppress with `!`.

## Author Payloads To Convention

JSON we build must follow the codebase's casing rules.

- JSON keys we *author* (in `Requests/Factory/*.cs`) are camelCase; two-letter acronyms
  mid-identifier capitalise only the first letter (`retrieveHhAccountRequest`,
  `xmlDocument`), matching .NET's `IOException → ioException` rule.
- JSON keys we *read* from a third-party response stay exactly as the server emits them —
  match the wire format.
- SOAP/XML payloads go through the existing XML serializers and the `XsdValidation` utility
  where schema validation applies; keep partner-specific XML shapes behind the wrapper, not
  in test bodies.

## Environments And Contracts

- Select environment and host through `Config` (see [Test Data Principles](test-data-principles.md)
  on base URLs) — never hard-code environment URLs, tenant IDs, or subscription keys.
- Treat downstream contracts as external: assert on the documented field, version-tolerant
  where the service is, and surface the full response in failure messages.
- Keep correlation/identifier values flowing through wrappers where the established pattern
  passes them, so failures are traceable.

> Reference: [`AGENTS.md` § Design conventions](../../AGENTS.md#design-conventions),
> [`TEST_DATA_STRATEGY.md`](../TEST_DATA_STRATEGY.md),
> [`skills/api-ui-automation/references/api-automation.md`](../../skills/api-ui-automation/references/api-automation.md).
