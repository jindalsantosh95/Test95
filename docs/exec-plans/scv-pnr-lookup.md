# SCV PNR booking lookup

Return SCV booking details through public Trips GraphQL, using the existing
PNR validation repository and Order Data service boundary.

- Branch: `codex/scv-pnr-lookup`, based on `origin/main`.
- Preserve `servicingValidatePnrOrder`, `/svc/orders/search` and live Trips queries.
- Add `/scv/orders/previous-trips` and `HistoricTripDetails` using existing
  `TripDetailsResponse` and `TripsBooking` contracts.
- Require booking creation date (`ddMMMyy`); parameterize the exact `OrderID = PNR + bookingDate` lookup.
- Match both names to one passenger. Wrong dates, names and absent bookings return an empty list.
- Require the supplied booking date to be before today (UTC). Do not filter by
  segment arrivals or cancellation status. GraphQL excludes groups by default using
  the existing ExcludeGroupBookings setting and retains all passengers on accepted
  bookings, matching TripsQuery. This does not establish completed travel.
- Project trip fields before serialization, excluding payment data and raw SCV
  storage graphs. GraphQL's inherited payment field remains null.

Implementation and acceptance tests are complete; PR merge and deployment remain
outstanding. See the [sequence goal](../sequence-diagrams/vaa-eapi-graphql-historic-trip-details.md)
for the current contract and rollout order.

## Latest verification — 2026-10-09

This table is the current passenger-parity run. Earlier runs are superseded;
the decision history below does not represent additional current test results.
Coverage includes referenced assemblies and is not feature-only coverage.

| Scope | Passed / total | Line coverage | Branch coverage | Run-scoped evidence |
| --- | --- | --- | --- | --- |
| HistoricTripDetailsQueryTests and TripDetailsListQueryTests | 37 / 37 | 28.20% (3392/12030) | 21.45% (1214/5660) | `TestResults/scv-passengers-trips-final/green.trx` |
| Full Order Data test project | 124 / 124 | 17.24% (1747/10135) | 8.98% (441/4910) | `TestResults/scv-passengers-order/green.trx` |

Both current-run TRX analyses found no failures. Evidence is retained locally in
ignored TestResults directories and is not committed. Enabled-group rejection and reservation-type assertions failed before the production fix
(`TestResults/scv-passengers-red/red.trx`). The final tests verify both setting values
and preservation of all passengers. An empty-setting test was removed because an
explicitly cached empty value is not the same as an absent setting in ConfigReader.

Required harness: `scripts/verify.ps1 -ProjectPath <owning-production-project.csproj> -SkipBuild -Base origin/main`
passed for both Trips and Order Data on this passenger-parity run. SIT settings were checked read-only, but no live booking
was retrieved. Mermaid render validation could not run because this session has no npx executable.
Deploy Order Data and
refresh APIM before GraphQL; deployed-environment verification remains part of rollout.

## Implementation and review decisions

- On 2026-10-09 SIT QA reported blank scheduled arrivals throughout the connected
  container. The user replaced the completed-travel requirement with a past booking
  date check, exact OrderID lookup and same-passenger name validation. Arrival,
  cancellation and group filters were removed at that point; future travel may be returned.
- The subsequent passenger-parity request retains all passengers and aligns group
  rejection with TripsQuery: GraphQL reads the existing ExcludeGroupBookings setting
  (default true), using reservationType projected from SCV customer group markers.
  The setting can allow groups without changing the passenger projection.

- Exact top-level `OrderID` casing matches Cosmos documents; nested
  `OrderDetails.OrderId` is not the lookup key.
- Existing OAuth, subscription and Cosmos configuration is reused. No new secrets
  or settings are required. Connector failures redact request/response bodies and
  downstream requests carry no-cache/no-store.
- Stored SCV timestamp aliases and the repository's shared JSON round-trip retain
  ISO values for response mapping.
- On 2026-10-09 the user confirmed the existing TripsBooking contract. This
  supersedes the original full-SCV response and history-projection design.
  Payment records, raw tickets/ancillaries/history, storage metadata and untyped
  fare blobs are excluded; the duplicate public SCV DTOs were removed.
- Historic trip fields are mapped explicitly, retaining passenger and flight data
  without inventing live-service enrichment. Missing optional dates remain null.
- Duration review: input-only aliases map Order Data's totalSliceDuration and
  totalSegmentDuration into the shared public duration fields. Both GraphQL
  selections are asserted through the registered query/provider and HTTP harness.
- Leg review: an input-only Destination_Airport_Code alias retains persisted SCV
  leg destinations. The regression exercises the real mapper with stored-shape JSON.
- Shared configuration review: Historic Trips tests use the same canonical Order
  Data URL without a trailing slash as Setup, avoiding double-slash mock requests.

## Test approach

GraphQL resolves production query/provider implementations and the real HTTP
connector through Trips Startup. Checked-in `.mock` resources use the shared
xUnit HTTP factory. Order Data resolves the real handler through DI.
The only local boundary substitutes are synthetic Cosmos repository data, a
fixed clock and the existing connector-backed OAuth adapter. No new fakes were
introduced for these review fixes. Initial implementation tests are regression
evidence; later red-before-green runs are identified explicitly above.

The supplied Azure DevOps xUnit wiki was inaccessible in the original session;
alignment was checked against maintained repository guidance and existing tests.
