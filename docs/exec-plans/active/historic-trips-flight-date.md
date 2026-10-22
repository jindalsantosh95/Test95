# Find Trip By Flight Date lookup

- Status: implementation complete; awaiting PR review and rollout
- Owner: Codex
- Created: 2026-10-15
- Scope: Retail Order Data, Trips GraphQL, Trips automation and contract documentation

## Objective

Replace the customer-supplied booking date with a scheduled local flight departure date.
Expose FindTripByFlightDate and POST /scv/orders/by-flight-date. Return a booking only when PNR, flight date and same-passenger names identify exactly one record.

## Acceptance Criteria

- Required flightDate uses ddMMMyy; any valid past, present or future date is accepted.
- Project candidate OrderIDs by PNR prefix, filter exact candidates by flight date and passenger, then retrieve the sole match.
- Zero or multiple matching records return an empty list; failures remain errors.
- Preserve response booking dates, passenger projection, group settings and payment exclusions.
- Update examples, automation and unit tests; run scoped harness checks and open a draft PR to main.

## Decisions

- 2026-10-15: Match persisted ScheduledDepartureTime.Local_Date_Time, without a UTC fallback.
- 2026-10-15: Prefix matching can fan out; exact OrderID equality routes only when the deployed partition path is /OrderID. Measure costs during rollout.
- 2026-10-15: Evaluate ambiguity before group filtering. Recheck final document identity, departure and names before projection.
- 2026-10-15: Rename the input explicitly; consumers and APIM must deploy with the revised contract. Existing response bookingDate retains its meaning.

## Plan

1. Update Order Data lookup and contract.
2. Update GraphQL, examples, regression data and documentation.
3. Run scoped tests and harness checks, review the diff, and open a draft PR.

## Broadened scope

- Rename the public query and route, request templates, tests and examples to FindTripByFlightDate and /scv/orders/by-flight-date.
- Remove past-only validation and its unused TimeProvider dependency. Preserve exactly-one-match rules and SCV response protections.
- Retain sensitive diagnostic classification for both the renamed route and the legacy path.
- Extend validation coverage to yesterday, today and tomorrow; deployed automation permits any flight date.

## Validation of initial flight-date change

- Retail Order Data: 132/132 tests passed; TRX analysis found no failures. Used dotnet test with TestingPlatformDotnetTestSupport=false and UseMicrosoftTestingPlatformRunner=false to enable VSTest TRX and XPlat coverage without changing project configuration.
- Historic Trips GraphQL: 17/17 tests passed; TRX analysis found no failures.
- Aggregate coverage: Order Data 17.4% lines / 9.34% branches; filtered GraphQL run 9.25% lines / 5.89% branches. These include referenced assemblies, not only changed code.
- Trips automation project Release build passed with existing warnings; deployed SIT regression was not run.
- scripts/verify.ps1 -ProjectPath <project> -SkipBuild -Base origin/main passed for Retail Order Data, Trips GraphQL and Trips automation.
- git diff --check passed; sequence diagram status passed through the harness.
- Mermaid render validation could not run: npx is unavailable on PATH. No runtime installation was changed.
- Live Cosmos queries, deployed partition routing and RU/latency remain SIT rollout checks. Unit tests substitute the Cosmos repository boundary; they do not execute SQL against Cosmos.

## Outcome

Implemented the explicit flightDate input, candidate-ID lookup, exact-ID/date/name filtering, and empty responses for ambiguity. Contract examples and automation use FindTripByFlightDate and /scv/orders/by-flight-date for past, current and future bookings. Draft PR targets main; merge/deployment remain outside this task.

## Validation of broadened scope

- Retail Order Data: 132/132 passed; TRX analysis reported no failures. Aggregate coverage: 17.36% lines, 9.25% branches.
- FindTripByFlightDate GraphQL: 18/18 passed; TRX analysis reported no failures. Aggregate coverage: 9.24% lines, 5.89% branches.
- Trips automation Release build passed with existing warnings. Live SIT tests were not run.
- Generated Functions metadata contains ScvFlightDateLookup and scv/orders/by-flight-date.
- Scoped harness checks passed for Order Data, Trips GraphQL, GraphQL infrastructure and Trips automation using scripts/verify.ps1 -ProjectPath <project> -SkipBuild -Base origin/main.
- The draft PR remains #2472 against main; its operation/route rename requires coordinated client and APIM updates.
