# Workflow Failure Tracing

This guide describes the optional failure-tracing handoff between GitHub Actions test workflows and Azure Application Insights. The workflow captures safe identifiers from failed TRX results; an engineer then enriches those failures locally using their existing GitHub CLI and Azure CLI sessions.

The initial integration is the Book GraphQL regression job in `.github/workflows/search-book-smoke.yml`.

## Design

The integration deliberately separates CI evidence collection from Azure access:

1. The test job writes normal TRX results.
2. `scripts/analyse-test-run-results.ps1` analyses failed tests and, when enabled, writes a failure-only trace manifest.
3. The workflow uploads the manifest with its existing test-results artifact and adds the local trace command to the GitHub job summary.
4. Slack contains only a link to the failed workflow; it does not contain trace output.
5. An engineer runs `scripts/trace-workflow-failures.ps1` locally. The script downloads the artifact with `gh`, then queries Azure using the signed-in `az` identity through the Azure correlation-trace helper.

This means GitHub Actions does not require Azure credentials, workload identity, Application Insights permissions, or new repository secrets.

## Safety Boundaries

- Tracing is supported only for `dev`, `sit`, and `uat`.
- Both manifest generation and local trace consumption reject production.
- A manifest is generated only when at least one test failed.
- The manifest contains failed-test diagnostics and trace identifiers, not Azure query results.
- Basket IDs are excluded from trace-identifier extraction.
- Azure results remain on the operator's workstation unless they explicitly choose to share them.
- The existing workflow artifact retention and access controls apply to the manifest.

Before extending the extractor, treat TRX text as potentially sensitive. Do not add payment data, credentials, tokens, customer PII, or complete request and response payloads to the manifest.

## Prerequisites

The operator needs:

- PowerShell 7;
- GitHub CLI authenticated with permission to read the workflow run and its artifacts;
- Azure CLI authenticated to a subscription with read access to the relevant Application Insights or Log Analytics resources;
- Python 3;
- the `azure-correlation-trace` helper at `~/.codex/skills/azure-correlation-trace/scripts/trace_app_insights.py`.

The helper path can instead be supplied with `-AzureTraceScriptPath` or the `AZURE_CORRELATION_TRACE_SCRIPT` environment variable.

Authenticate when required:

```powershell
gh auth login
az login
az account set --subscription "<subscription-name-or-id>"
```

Azure authorization is evaluated against the signed-in user. The user needs query/read access for each target environment; the workflow itself needs no Azure authorization.

## Running A Trace

From the repository root, use the run ID shown in the workflow URL:

```powershell
./scripts/trace-workflow-failures.ps1 `
  -RunId 38672399204 `
  -Environment sit
```

The environment is optional when the downloaded artifacts identify it unambiguously. Supplying it is recommended when a matrix run contains several environments.

By default, one representative identifier is traced for each grouped incident. This avoids issuing duplicate Azure queries when many scenarios fail for the same underlying reason. To trace every extracted identifier:

```powershell
./scripts/trace-workflow-failures.ps1 `
  -RunId 38672399204 `
  -Environment sit `
  -TraceAllIdentifiers
```

The command creates a temporary output directory unless `-OutputDirectory` is supplied. It writes:

- `azure-trace-summary.md`, containing the per-identifier outcome;
- one raw JSON trace file per queried identifier;
- the downloaded workflow artifacts used as source evidence.

An Azure helper exit code indicating no matching telemetry is reported as a valid no-match result. Authentication, configuration, or query failures are reported separately.

The Azure lookback is derived from the earliest failed-test timestamp in each manifest, with a one-day buffer and a minimum of two days. When timestamps are unavailable it defaults to 30 days. An operator can override the derived value when needed:

```powershell
./scripts/trace-workflow-failures.ps1 `
  -RunId 38672399204 `
  -Environment sit `
  -Lookback 14d
```

## Trace Manifest Contract

The manifest is JSON with `schemaVersion` set to `1`. Its workflow metadata includes the repository, run ID, run URL, suite, environment, generation time, and failure count.

Each failure contains:

- TRX source, test, and scenario names;
- test start/end timestamps and duration;
- GraphQL operation name when detected;
- classified signal and suggested owner;
- a bounded diagnostic message;
- extracted correlation, error, request, or operation IDs;
- an incident key used to group equivalent failures.

The `incidents` collection provides grouped scenario names and identifiers so local tracing can query one representative identifier per failure cluster.

## Adding Another Workflow

Use the following pattern for workflows that produce TRX output.

### 1. Add An Optional Input

For manually dispatched workflows, add a boolean input defaulting to enabled:

```yaml
enable_failure_trace_manifest:
  description: "Generate a failure-only manifest for optional local Azure tracing"
  required: true
  default: true
  type: boolean
```

Scheduled runs can enable the feature automatically. Expose the resolved value to the test job, for example:

```yaml
env:
  ENABLE_FAILURE_TRACE_MANIFEST: ${{ github.event_name == 'schedule' || inputs.enable_failure_trace_manifest }}
```

### 2. Use The Workflow's Resolved Environment

Pass the environment already used by the job or matrix to the analyser. Do not introduce a separate trace-environment input because it can drift from the system under test.

```powershell
$analysisParameters = @{
  ResultsDirectory = "TestResults"
  SuiteName = "<suite name>"
  EnvironmentName = $env:env
}

if ($failed -gt 0 -and $env:ENABLE_FAILURE_TRACE_MANIFEST -eq "true") {
  $analysisParameters.TraceManifestPath = "TestResults/trace-manifest-<suite>-$env:env.json"
  $analysisParameters.WorkflowRunUrl = "${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}"
}

$analysis = & "scripts/analyse-test-run-results.ps1" @analysisParameters
```

Only pass `TraceManifestPath` after the test result confirms a failure. The analyser provides a second enforcement layer and will not create a manifest for a passing TRX.

### 3. Upload The Manifest With Existing Results

Add the manifest glob to the workflow's existing test-results artifact:

```yaml
path: |
  TestResults/*.trx
  TestResults/trace-manifest-*.json
```

The artifact name must match the `*graphql*regression-results-*` pattern currently used by `trace-workflow-failures.ps1`. If a new workflow uses a different artifact convention, either align its name or extend the local downloader with a backwards-compatible artifact pattern.

### 4. Keep Notifications Small

Link Slack or other notifications to the workflow run. Put the detailed failure groups and local trace command in `$GITHUB_STEP_SUMMARY`; do not copy Azure trace results into chat notifications.

### 5. Verify The Integration

At minimum, prove that:

- a failed DEV/SIT/UAT TRX creates a manifest;
- a passing TRX creates no manifest;
- production is rejected;
- correlation/error IDs are extracted while business entity IDs are excluded;
- the manifest is included in the uploaded artifact;
- disabling the workflow option suppresses the manifest.

Use `scripts/test-test-run-trace-manifest.ps1` as the contract test and add workflow-specific fixtures only when the new suite emits a materially different TRX shape.

## Future Extensions

Good incremental extensions include:

- accepting an explicit artifact-name pattern in the local command so non-GraphQL suites can use the same implementation;
- adding extractors for suite-specific trace header names while retaining the identifier allowlist;
- rendering Azure findings into a separate local HTML or Markdown report;
- linking a failed scenario directly to its GitHub job log when stable job/step URLs are available;
- promoting the manifest schema into a shared test-results contract once several workflows use it.

Avoid moving Azure queries into GitHub Actions unless there is an approved non-production workload identity, least-privilege telemetry role assignment, secretless authentication, audit ownership, and an explicit decision about whether enriched telemetry is safe to retain as a CI artifact.

## Troubleshooting

| Symptom | Likely cause | Action |
| --- | --- | --- |
| No trace manifest found | Run predates the feature, all tests passed, or the option was disabled | Inspect the job summary and artifact contents. |
| GitHub download fails | `gh` is not authenticated or cannot read the repository/run | Run `gh auth status` and confirm repository access. |
| Azure authorization fails | Signed-in user lacks telemetry query access or selected the wrong subscription | Run `az account show`, select the correct subscription, and request read/query access for the environment. |
| No matching telemetry | Identifier was not emitted to the queried resources or fell outside the lookback window | Confirm the environment, timestamp, and identifier in the TRX diagnostics. |
| Helper not found | Personal Codex skill is installed elsewhere | Pass `-AzureTraceScriptPath` or set `AZURE_CORRELATION_TRACE_SCRIPT`. |

## References

- `scripts/analyse-test-run-results.ps1`
- `scripts/trace-workflow-failures.ps1`
- `scripts/test-test-run-trace-manifest.ps1`
- `.github/workflows/search-book-smoke.yml`
- [Harness Capabilities Matrix](harness-capabilities-matrix.md)
