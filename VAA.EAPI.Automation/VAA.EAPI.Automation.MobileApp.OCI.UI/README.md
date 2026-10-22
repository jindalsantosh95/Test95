# Mobile OCI automation

This project owns native Online Check-In journeys. It deliberately reuses the Appium fixture,
startup handling, BrowserStack diagnostics, Core Storage Table connection, and established OCI
GraphQL clients from the existing EAPI automation projects.

`OCI-TC01` mirrors WDIO's single-adult priority-route smoke journey. Candidate selection reads
both `OnlineCheckInMobileAppData` and `OCIPNRs`, keeps single-passenger departures between 80
minutes and 24 hours away, and tries routes in LHR, USA, then any-destination order. Candidates
must pass Trips Booking eligibility, CheckIn and SavePassport before the UI journey begins.

Run the draft scenario from the OCI mobile project:

```bash
dotnet test VAA.EAPI.Automation.MobileApp.OCI.UI.csproj \
  --filter "FullyQualifiedName~OnlineCheckIn_TC01"
```

`appsettings.example.json` is the committed non-secret baseline and is copied to the test output
as `appsettings.json`. This project deliberately does not reference or copy MobileApp.UI's
gitignored local settings file. Introduce a project-owned local configuration contract before the
OCI scenario is promoted from `NotReady` into a live local or BrowserStack execution lane.

Each additional live OCI scenario must use its own concrete test class so BrowserStack can run
sessions in parallel. Keep API/data preparation out of page objects, use stable app-owned
accessibility IDs, and use bounded observable waits rather than sleeps.

Known app-readiness gap: Add Trip's Next button does not currently expose a dedicated iOS
accessibility ID. `OciCheckInPage` contains the same screen-scoped predicate fallback used by
WDIO, with an explicit removal condition once the app supplies that ID.

## Conversion approach and framework comparison

The objective is not to prefer .NET over TypeScript as a language. WDIO proves that these mobile
journeys are automatable and remains the current coverage baseline. It also contains valuable OCI
domain knowledge, including storage-table selection, API preparation, route prioritisation and
legacy UI fallbacks. That knowledge should be migrated deliberately rather than discarded.

The architectural concern is duplicated harnessing. WDIO and EAPI both implement Azure Storage,
secret retrieval, GraphQL clients, environment configuration, data filtering, retry behaviour and
failure diagnostics. EAPI already owns these capabilities for backend automation, so new mobile
projects should reuse them instead of creating another mobile-specific implementation. Shared
clients or helpers should be extracted when reuse is not currently clean; copying backend logic
into a UI test project is not the intended end state.

Search and Book demonstrates the preferred delivery model: frontend changes include stable,
app-owned accessibility identifiers and automation-readiness review. This benefits both WDIO and
EAPI, but it allows the EAPI Appium tests to remain small, deterministic and suitable for parallel
and agent-authored execution. Framework comparisons must therefore measure more than elapsed UI
time: include implementation effort, repeated-run stability, parallel stability, diagnosis time,
backend/data reuse, onboarding and maintenance after UI changes.

Legacy OCI screens are not yet at the same readiness level. WDIO currently succeeds by trying
multiple selectors, alternate navigation routes and screen-specific fallbacks. Those fallbacks are
useful evidence of missing app contracts, but reproducing all of them would preserve the same
maintenance debt. The preferred order is:

1. Add stable identifiers for the trip container/PNR, its scoped Check In action, Continue to Check
   In and the remaining OCI controls.
2. Select a trip by PNR and interact only with controls scoped to that trip.
3. Use a narrowly scoped, documented fallback only when a frontend change cannot be delivered in
   the same increment.
4. Give every fallback a removal condition and retain the stable identifier as the primary lookup.

The current conversion test proves storage discovery, Trips Booking eligibility, CheckIn and
SavePassport preparation, Add Trip and trip verification. Its local end-to-end run is not yet a
passing test: after the trip is added, the current app does not reliably expose the correct scoped
Check In/Continue to Check In transition to Appium. Keep the scenario out of required CI lanes
until that app contract is implemented and the boarding-pass assertion passes repeatedly.
