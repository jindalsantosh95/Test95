# Data provisioning payment automation

`VAA.EAPI.Automation.Utility.DataProvisioning` creates synthetic PNR test data from an external JSON manifest by using the shared Book API orchestration. Frictionless card scenarios use the payment APIs by default; PayPal and explicit HPP runs retain the Playwright implementation. It is currently opt-in: no Book workflow, OCI release, Trips job, schedule, or test trait invokes it.

The existing `.github/workflows/qa-trips-pnr-creation.yml` workflow still runs the legacy Trips xUnit PNR-creation categories. It does not invoke this manifest runner. Keep that workflow in place until the new runner has completed a controlled, side-by-side rollout.

## Why it is separate

The runner is data provisioning rather than regression testing. Keeping it in a `Utility.*` project allows it to consume the existing Book orchestration without Book, Trips, or OCI depending on the runner. The legacy `Utility.PNRCreation` remains unchanged because it is an NDC/CSV-specific tool.

The shared `PnrProvisioner` remains in Book during the pilot to avoid moving code used by existing jobs. If the pilot proves stable, its API orchestration can later be extracted into a dedicated reusable domain library. That extraction is deliberately not part of the pilot.

## Prerequisites and settings

The machine or agent must have:

- .NET 10 SDK, plus Google Chrome when the manifest contains PayPal or selects HPP card payment;
- network/VPN access to the selected EAPI and direct HPP endpoints;
- a populated, gitignored settings file or equivalent environment variables;
- access to the existing Azure storage account when `tableOutputs` are enabled.

The runner uses the same configuration contract as Book automation. The required secret names are:

- `KeyVaultUrl`
- `KeyVaultTenantId`
- `KeyVaultClientId`
- `KeyVaultClientSecret`
- `HppVaaDigitalFirstToken`
- `HppDeltaEnterprisePaymentsToken`
- `AzureStoragePnrCreationsConnectionString` when writing storage rows

The target environment is selected by `env` (`dev`, `sit`, or `uat`). Endpoint paths and base URLs continue to use the established Book configuration names. Do not commit a populated settings file or print its values.

## Run locally against SIT

Connect to the required network and provide the same non-secret local settings used by Book automation. From the `VAA.EAPI.Automation` solution directory on Windows:

```powershell
dotnet run --project .\VAA.EAPI.Automation.Utility.DataProvisioning\VAA.EAPI.Automation.Utility.DataProvisioning.csproj -- .\VAA.EAPI.Automation.Utility.DataProvisioning\example.sit.json C:\path\to\local.settings.local.json
```

From macOS or Linux, run from the repository root:

```bash
dotnet run --project ./VAA.EAPI.Automation/VAA.EAPI.Automation.Utility.DataProvisioning/VAA.EAPI.Automation.Utility.DataProvisioning.csproj -- \
  ./VAA.EAPI.Automation/VAA.EAPI.Automation.Utility.DataProvisioning/example.sit.json \
  /absolute/path/to/local.settings.local.json
```

From PowerShell at the repository root, include the `VAA.EAPI.Automation` solution directory in the paths:

```powershell
dotnet run --project .\VAA.EAPI.Automation\VAA.EAPI.Automation.Utility.DataProvisioning\VAA.EAPI.Automation.Utility.DataProvisioning.csproj -- `
  .\VAA.EAPI.Automation\VAA.EAPI.Automation.Utility.DataProvisioning\example.sit.json `
  C:\absolute\path\to\local.settings.local.json
```

The manifest and settings arguments can be absolute or relative to the directory where the command is invoked. The runner resolves them before switching to the compiled request-template directory.

`cardPaymentMode` controls ordinary card submission for the complete run. It defaults to `Api`; set it to `Hpp` to route cards through the existing headless Playwright flow. PayPal always uses Playwright regardless of this setting. API mode is intended only for frictionless non-production test cards. A challenged 3DS response fails at `HppSubmit` and is not automatically retried through HPP, because resubmitting payment after an ambiguous response could duplicate payment activity.

```json
{
  "maxConcurrency": 2,
  "cardPaymentMode": "Api",
  "dataSets": []
}
```

The manifest may live outside the repository. Do not commit credentials, loyalty passwords, real passenger information, or production account data.

Two reusable baselines are provided:

- `defaults/trips.json` is generated from every current Trips provider: 442 scenarios comprising 72 Flying Club, 88 Glass, 10 reward, 30 partner-loyalty and 242 seat-setup rows.
- `defaults/oci.json` is generated from every row exposed by `OCIPnrDataCreationTestCases`: 516 scenarios across 24 categories, including each of the three departure offsets.

Use the smaller manifests for the initial SIT proving run:

- `defaults/pilots/trips.json`: five scenarios covering Flying Club, Glass, reward, partner-loyalty and seat-setup data.
- `defaults/pilots/oci.json`: nine scenarios covering revenue, seats, reward, insurance, partner, multi-city, connection filtering, PayPal/non-seamless and seasonal data.

Both defaults are complete catalogues rather than samples. Re-running them resolves ordinary dates relative to the run date and resolves seasonal dates only within the configured service periods. An expired seasonal row is reported as `seasonal-unavailable` without attempting a booking; refresh the shared seasonal schedule before the next service period if that route should resume. The Trips seat-setup rows deliberately retain `seats: false`: those PNRs are created without an assigned seat so downstream Trips seat journeys have data to operate on.

The OCI catalogue represents revenue, reward, insurance, three/four-slice multi-city, distinct inbound fare/carrier, direct, direct-with-stop, one/two-stop, long-layover, same-day return, TSA/ESTA passenger names, seamless/non-seamless, seasonal routes, specific seat products, card and PayPal journeys. It is generated from the existing OCI provider through `OciDefaultManifestFactory`, which prevents a manually maintained second list drifting from the release data. Existing OCI jobs remain authoritative until every category has passed a side-by-side SIT comparison and its required storage output has been verified.

Regenerate the checked-in OCI JSON after changing the existing OCI provider:

```powershell
dotnet run --project .\VAA.EAPI.Automation.Utility.DataProvisioning\VAA.EAPI.Automation.Utility.DataProvisioning.csproj -- --write-oci-default .\VAA.EAPI.Automation.Utility.DataProvisioning\defaults\oci.json
```

Regenerate the Trips JSON after changing a Trips provider:

```powershell
dotnet run --project .\VAA.EAPI.Automation.Utility.DataProvisioning\VAA.EAPI.Automation.Utility.DataProvisioning.csproj -- --write-trips-default .\VAA.EAPI.Automation.Utility.DataProvisioning\defaults\trips.json
```

The `MobileAppRevenuePNRCreation` and `MobileAppRedemptionPNRCreation` classes inherit the same OCI scenario catalogue and change only the storage destination. They do not require a duplicate set of manifest scenarios.

Validate a manifest without loading environment settings, opening Chrome, creating a booking, or writing storage:

```powershell
dotnet run --project .\VAA.EAPI.Automation\VAA.EAPI.Automation.Utility.DataProvisioning\VAA.EAPI.Automation.Utility.DataProvisioning.csproj -- .\VAA.EAPI.Automation\VAA.EAPI.Automation.Utility.DataProvisioning\defaults\oci.json --validate-only
```

`maxConcurrency` controls complete booking journeys. Guest journeys can run up to that limit. Logged-in journeys are deliberately serialised for their full lifecycle so two reward bookings cannot select and update the same loyalty account concurrently. Playwright sessions used by PayPal or `cardPaymentMode: "Hpp"` are additionally protected by `HPP_PLAYWRIGHT_MAX_CONCURRENT_SESSIONS` (default `2`). Start with both configured limits at `2` on a shared VM. Unknown command-line options and extra positional arguments are rejected before settings are loaded or bookings begin.

The process exits `0` when every scenario creates a PNR, encounters accepted no-inventory, or has no future date in its configured seasonal service period; `1` when a scenario fails; and `2` for invalid input. Its JSON output includes data-set and scenario names, status, PNR, storage tables, and the failed provisioning stage. Passenger names are omitted by default because stdout is commonly retained in CI logs; use `--include-passenger-names` only for an explicitly protected local output. Failure output contains sanitised exception types rather than GraphQL/payment payloads.

Each scenario status has a specific meaning:

- `created`: order creation completed and every configured storage sink succeeded;
- `no-inventory`: the established accepted sell/no-inventory path produced no PNR;
- `seasonal-unavailable`: the route is known but its configured service periods contain no future departure date, so no booking was attempted;
- `failed`: the named stage failed; other scenarios continue and are still reported.

## Pilot safety

- API card submission is selected by default only by this runner; other provisioner consumers retain their existing HPP default.
- PayPal and `cardPaymentMode: "Hpp"` continue to use Playwright.
- Booking creation is not retried because retrying after HPP could duplicate a charge or PNR.
- Storage writes are disabled when a data set has no `tableOutputs`, so the checked-in pilot manifests do not modify OCI or Trips tables.
- Run only the supplied manifest from the pilot branch. Existing release definitions continue unchanged.
- Card/PayPal submission and order creation are never retried. Storage writes alone use bounded retries because they are idempotent for the same PNR row.

## Storage behaviour

Storage is configured per data set. Omit `tableOutputs`, or supply an empty array, to create bookings without publishing them. Each enabled output requires both the existing record `schema` and the exact Azure `tableName`:

The following is a `tableOutputs` fragment to merge into a populated data set; it is not a complete runnable manifest:

```json
{
  "name": "oci-revenue",
  "tableOutputs": [
    { "schema": "Oci", "tableName": "OCIPNRs" },
    { "schema": "OciMobileApp", "tableName": "OnlineCheckInMobileAppData" }
  ]
}
```

Supported schemas are `MobileApp`, `LoyaltyAccount`, `Oci`, `OciMobileApp`, and `SeatScenario`. They reuse the established entity mappings; the manifest controls only the destination table. A data set can publish each successfully created PNR to more than one table, as OCI currently does for its main and mobile-app consumers. OCI scenarios also require `storagePartitionKey`, which preserves the fixed keys used by existing readers while `name` remains unique for runner diagnostics. The generated OCI catalogues populate these legacy keys automatically. Azure table names and partition keys are validated before any booking starts. The storage connection remains the existing `AzureStoragePnrCreationsConnectionString` secret and is never placed in the manifest.

Typical destinations are:

| Consumer | Schema | Existing table |
| --- | --- | --- |
| Trips general PNR data | `MobileApp` | `MobileAppPnrs` |
| Trips partner-loyalty data | `LoyaltyAccount` | `MobileAppPnrsForLoyaltyAccounts` |
| Trips seat setup | `SeatScenario` | `MobileAppPnrsForSeatScenarios` |
| OCI backend data | `Oci` | `OCIPNRs` |
| OCI mobile-app data | `OciMobileApp` | `OnlineCheckInMobileAppData` |

Do not attach all Trips scenarios to one storage schema. The Flying Club/Glass, partner-loyalty and seat-setup groups have different consumers and must retain their established mappings when scheduled manifests are introduced.

## Scheduled workflow rollout

Trips and OCI should have separate thin workflows, both calling one reusable data-provisioning workflow. The reusable workflow should own checkout, .NET build, manifest validation, execution, sanitised job-summary generation, artifact upload and cleanup. The thin consumer workflows should own only their schedule, manual inputs, catalogue selection, runner labels and secret mapping.

The workflow contract should provide:

- manual `target_environment` selection, defaulting to `sit`;
- manual `catalogue` selection of `pilot` or `full`, defaulting to `pilot` during rollout;
- `workflow_dispatch` support from a branch for controlled proving;
- independent concurrency groups for Trips and OCI with `cancel-in-progress: false`;
- staggered execution that prevents separate Trips and OCI provisioning processes from running logged-in catalogues at the same time;
- a self-hosted Windows runner carrying `self-hosted`, `Windows`, and `eapi-automation` labels;
- Playwright Chrome with `HPP_PLAYWRIGHT_MAX_CONCURRENT_SESSIONS=2` initially;
- a JSON artifact and Markdown summary containing created/no-inventory/failed totals, PNRs, lead passenger names, storage destinations and failed stages;
- `if: always()` browser/process diagnostics and cleanup, without terminating browsers owned by another concurrent job;
- no automatic retry of failed booking/payment scenarios.

Use staggered schedules so Trips and OCI do not compete for Chrome and VM capacity. Until runtime measurements exist for the full 442- and 516-scenario catalogues, only the small pilot catalogues should be scheduled. A full catalogue must remain manual and storage-disabled until its scenario-to-table routing has been reviewed; running either full catalogue as currently checked in creates bookings but publishes no rows.

Recommended rollout:

1. Add the reusable workflow and manual-only Trips/OCI callers using the pilot manifests.
2. Prove each caller independently on SIT and verify its PNR rows in the intended tables.
3. Add staggered pilot schedules and observe runtime, browser count and downstream failure rate.
4. Split the full catalogues into correctly routed data sets and validate expected row counts.
5. Enable full scheduled creation only after the old and new outputs match; then retire the corresponding legacy creation job deliberately.
