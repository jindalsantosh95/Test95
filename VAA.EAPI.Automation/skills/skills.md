# Skills Index (VAA.EAPI.Automation)

This file indexes automation skills available in this solution area.

## Available skills

### api-ui-automation
- Path: [`api-ui-automation/SKILL.md`](./api-ui-automation/SKILL.md)
- Use for: API, GraphQL, SOAP, UI/mobile automation additions and updates.
- References:
  - [`api-ui-automation/references/api-automation.md`](./api-ui-automation/references/api-automation.md)
  - [`api-ui-automation/references/ui-automation.md`](./api-ui-automation/references/ui-automation.md)

### new-playwright-ui-project
- Path: [`new-playwright-ui-project/SKILL.md`](./new-playwright-ui-project/SKILL.md)
- Use for: creating a new C# Playwright UI project with the mandatory page-object architecture,
  safe setup scripts, configuration example, README, and solution registration.
- Run from this folder with
  `pwsh -NoProfile -File skills/new-playwright-ui-project/scripts/new-playwright-ui-project.ps1`.

### test-run-result-analysis
- Path: [`../../scripts/analyse-test-run-results.ps1`](../../scripts/analyse-test-run-results.ps1)
- Use for: post-test TRX analysis after local or CI runs for any test project in `VAA.EAPI.Automation.sln`.
- Notes:
  - The script is repository-root scoped so workflows can call it as `scripts/analyse-test-run-results.ps1`.
  - It recursively reads `.trx` files under the supplied results directory and reports generic evidence-rich failure analysis from TRX error, stack, stdout, stderr, and debug output.
  - Use `-TrxPath` for local single-run analysis when a results directory contains older `.trx` files.
  - It can be reused for unit tests or wider solution runs when those jobs publish TRX output.
  - Unit-test jobs outside `VAA.EAPI.Automation.sln` should also collect coverage with `--collect:"XPlat Code Coverage"`; the analyser appends a coverage table when Cobertura output exists.
  - When an agent runs a local test for a user, it should produce a named TRX and then run this script against `-TrxPath` or `-ResultsDirectory` unless the user explicitly asks for a raw/quick run.

## Maintenance notes
- Keep this index updated when adding/removing automation skills.
- Keep each skill scoped, actionable, and aligned with current project conventions.
