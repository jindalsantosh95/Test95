# Skills Index (eapi)

This file indexes repository-level skills, reusable workflow scripts, and solution-area skill indexes.

## Repository-level skills

### test-run-result-analysis
- Path: [`scripts/analyse-test-run-results.ps1`](./scripts/analyse-test-run-results.ps1)
- Use for: evidence-first post-test TRX analysis in GitHub Actions summaries or local runs for any TRX-producing test run in this repository.
- Scope: repository root. Workflows should call it as `scripts/analyse-test-run-results.ps1` from the checkout root.
- Notes:
  - Recursively reads `.trx` files under the supplied results directory for clean CI workspaces.
  - Supports `-TrxPath` for local single-run analysis when older TRX files may exist in `TestResults`.
  - Reports generic evidence-rich failure analysis from TRX error, stack, stdout, stderr, debug output, inferred signal, likely owner, and recommended next action.
  - Can be reused by automation, unit-test, smoke, regression, or wider solution jobs when they emit TRX output.
  - Unit-test jobs can also emit Cobertura coverage with `--collect:"XPlat Code Coverage"`; the analyser appends a coverage table when `coverage.cobertura.xml` exists under the results directory.

### eapi-unit-tests workflow
- Path: [`.github/workflows/eapi-unit-tests.yml`](./.github/workflows/eapi-unit-tests.yml)
- Use for: manually triggered full EAPI unit-test validation outside `VAA.EAPI.Automation.sln`.
- Behaviour:
  - Discovers non-automation projects that reference `Microsoft.NET.Test.Sdk`.
  - Excludes `VAA.EAPI.Automation`, `bin`, `obj`, and backup project files.
  - Runs each project with TRX output.
  - Collects XPlat Code Coverage by default and uploads the complete `TestResults` folder.
  - Appends the post-test analysis and coverage summary to the GitHub Actions step summary.

Example workflow usage:

```powershell
$analysis = & "scripts/analyse-test-run-results.ps1" `
  -ResultsDirectory "TestResults" `
  -SuiteName "EAPI Unit Tests" `
  -EnvironmentName "ci"

if (-not [string]::IsNullOrWhiteSpace($analysis)) {
  "`n$analysis" | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8 -Append
}
```


Local usage is manual. The analyser does not run automatically when a developer runs `dotnet test`; first emit TRX output, then call the script:

```powershell
dotnet test VAA.EAPI.Automation/VAA.EAPI.Automation.Book/VAA.EAPI.Automation.Book.csproj `
  --results-directory VAA.EAPI.Automation/VAA.EAPI.Automation.Book/TestResults `
  --logger "trx;LogFileName=book-local.trx"

scripts/analyse-test-run-results.ps1 `
  -TrxPath VAA.EAPI.Automation/VAA.EAPI.Automation.Book/TestResults/book-local.trx `
  -SuiteName "Book Local Run" `
  -EnvironmentName "local"
```

For local full unit-test validation outside automation, add `--collect:"XPlat Code Coverage"` so the analyser can include coverage.

## Solution-area indexes

### VAA.EAPI.Automation
- Path: [`VAA.EAPI.Automation/skills/skills.md`](./VAA.EAPI.Automation/skills/skills.md)
- Use for: automation-specific Codex skills and references.

## Maintenance notes
- Keep repository-level skills here when they are intended for workflows or projects outside a single solution area.
- Keep solution-specific skills in the relevant solution-area index and link them from this file.
