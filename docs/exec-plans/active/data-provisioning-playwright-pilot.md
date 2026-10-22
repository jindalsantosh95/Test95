# Playwright Data Provisioning Pilot

- Status: active
- Owner: Automation team
- Created: 2026-09-10
- Scope: Book PNR provisioner, standalone data-provisioning utility, documentation

## Objective

Provide an isolated, externally data-driven PNR creation runner that uses the shared Search/Book journey and the Playwright direct-HPP implementation without changing existing OCI, Trips, Book traits, schedules, or release jobs.

## Context

OCI and Trips currently duplicate booking orchestration inside test suites. A shared `PnrProvisioner` already exists in Book, but its payment implementation is fixed to Selenium and it has no standalone manifest-driven runner. The legacy `Utility.PNRCreation` is an NDC/CSV tool and is not an appropriate migration target.

## Acceptance Criteria

- A standalone utility accepts an external JSON manifest containing multiple PNR scenarios.
- Complete 442-row Trips and 516-row OCI manifests are available for repeatable manual runs.
- Five-row Trips and nine-row OCI pilot manifests allow representative SIT proving without launching the full catalogues.
- The utility uses Playwright explicitly for card and PayPal HPP journeys.
- Existing provisioner consumers retain Selenium by default.
- Scenario concurrency is bounded and results identify PNR, lead passenger, or failed stage.
- Storage outputs and destination table names can be selected independently for each data set, with no output as the safe default.
- No existing workflow, release definition, or test trait invokes the utility automatically.
- Usage and rollout guidance are documented.

## Plan

1. Add an explicit HPP execution option to the existing shared provisioner with a backwards-compatible default.
2. Add a top-layer data-provisioning console utility with manifest validation, bounded concurrency, and machine-readable results.
3. Expand the shared scenario contract for OCI insurance, multi-city, partner, offer-filter and special passenger journeys.
4. Add example/default manifests and operator documentation.
5. Build the affected projects and run architecture fitness checks.

## Decisions

- 2026-09-10: Keep the shared orchestration in Book for the pilot. Moving it now would affect existing consumers; the standalone runner belongs in a utility project and may reference Book without reversing the project graph.
- 2026-09-10: Select Playwright explicitly in the new runner. Existing consumers keep their current Selenium behaviour until individually migrated.
- 2026-09-10: Do not add a schedule or modify OCI/Trips release jobs during the pilot.
- 2026-09-10: Generate the OCI default from `OCIPnrDataCreationTestCases` so every existing route/offset remains represented and future provider changes can be regenerated rather than copied manually.
- 2026-09-10: Generate the Trips default from its core, reward, partner-loyalty and seat-setup providers for the same no-loss guarantee.
- 2026-09-11: Reuse the established table entity schemas while allowing each manifest data set to select one or more validated destination table names. Keep the existing vault-backed connection string out of manifests.

## Validation

- `dotnet build VAA.EAPI.Automation/VAA.EAPI.Automation.Utility.DataProvisioning/VAA.EAPI.Automation.Utility.DataProvisioning.csproj`
- `dotnet test VAA.EAPI.Automation/VAA.EAPI.Automation.ArchitectureTests/VAA.EAPI.Automation.ArchitectureTests.csproj`
- 2026-09-11 SIT proof: one guest OCI card journey and one guest Trips card journey completed through Playwright HPP; both returned PNRs and published successfully to their configured OCI/mobile-app and Trips tables.

## Outcome

The isolated runner, representative catalogues, Playwright HPP selection, per-data-set storage outputs and operator documentation are implemented. The initial two-journey SIT proof passed. Broader catalogue proving and migration of existing scheduled consumers remain deliberately outside this pilot PR.
