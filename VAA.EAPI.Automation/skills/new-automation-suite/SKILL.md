---
name: new-automation-suite
description: Onboard a new or existing EAPI .NET automation project into scheduled GitHub Actions using the manifest-driven reusable runner. Use when creating, scheduling, or standardising an automation suite; do not use for authoring the tests themselves.
---

# New Automation Suite

Use the deterministic scaffolder instead of copying an existing workflow. Read
[`../../docs/CI_PIPELINE_MAP.md`](../../docs/CI_PIPELINE_MAP.md) before changing execution ownership.

## Establish the contract

Identify these facts from the project and user request:

- suite display name, lowercase kebab-case slug, and owning team;
- project-relative `.csproj` path;
- existing xUnit categories that define the suite;
- schedule, target environments, timeout, and runner requirements;
- repository variables and secrets required at runtime.

Never invent secret values or commit them. Ask only for settings that cannot be derived from the
project, existing repository conventions, or the requested behaviour.

## Scaffold

Run from the repository root:

```pwsh
./scripts/new-automation-suite.ps1 `
  -Name "Offers Regression" `
  -Slug "offers-regression" `
  -Owner "Offers" `
  -ProjectPath "VAA.EAPI.Automation/VAA.EAPI.Automation.Offers/VAA.EAPI.Automation.Offers.csproj" `
  -Categories @("Offers-Regression") `
  -Cron @("0 5 * * 1-5") `
  -RequiredVariables @("BASE_URL")
```

The script refuses to overwrite files and creates:

- `config/automation-suites/<slug>.json`, the durable suite contract;
- `.github/workflows/qa-<slug>.yml`, the thin trigger and secret mapping.

Generated workflow display names use the repository-standard `Automation -` prefix so the Actions
navigation groups them consistently.

## Finish safely

1. Review the generated manifest against `config/automation-suites/schema.json`.
2. Keep schedules and manual inputs in the thin workflow; keep execution settings in the manifest.
3. Add custom summary or Slack configuration only when the defaults cannot represent the suite.
4. Update `VAA.EAPI.Automation/docs/CI_PIPELINE_MAP.md` with the new operational sensor.
5. Report the variable and secret names that an administrator must provision, without requesting values in chat.
6. Validate the manifest with `scripts/resolve-automation-suite.ps1` and parse the generated YAML.
7. Use manual dispatch for the first live run; do not trigger environment-dependent tests without authorization.
