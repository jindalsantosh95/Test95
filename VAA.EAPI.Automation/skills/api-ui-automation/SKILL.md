---
name: api-ui-automation
description: Accelerate creation and maintenance of API and UI/mobile automation in VAA.EAPI.Automation (.NET). Use when adding or updating API tests, GraphQL/microservice request helpers, UI/mobile page objects, step definitions, fixtures, and test-data setup.
---

# API/UI Automation Skill (VAA.EAPI.Automation)

## When to use this skill
Use this skill when the task touches any project in `VAA.EAPI.Automation.sln`, especially:

- API/GraphQL/SOAP automation suites (e.g., `MyAccount`, `Trips`, `CRM`, `Book`).
- UI/mobile automation suites (e.g., `MobileApp.UI`, `MyAccount.UI`, `MarketingLoyalty.UI`).
- Shared automation framework/core helpers.

## Required discovery first
1. Identify the target project folder and feature area.
2. Locate the nearest existing pattern (test, helper, page object, fixture).
3. Reuse existing abstraction layers before adding new ones.

## Execution baseline
Run from `VAA.EAPI.Automation/`:

```bash
dotnet restore VAA.EAPI.Automation.sln
dotnet build VAA.EAPI.Automation.sln -c Release
```

Run targeted tests for changed scope:

```bash
dotnet test <project>/<project>.csproj -c Release
dotnet test <project>/<project>.csproj -c Release --filter "Category=<existing-trait>"
```

When running tests for a user, prefer producing a named TRX and then run, or ask the
user whether to run, the repo-root post-test analyser against that exact TRX:

```pwsh
dotnet test <project>/<project>.csproj -c Release --filter "<filter>" --results-directory TestResults\<run-name> --logger "trx;LogFileName=<run-name>.trx"
..\scripts\analyse-test-run-results.ps1 -TrxPath TestResults\<run-name>\<run-name>.trx -SuiteName "<suite>" -EnvironmentName <env>
```

## API automation workflow
1. Reuse request/endpoint wrappers and helper methods before writing direct HTTP calls.
2. Keep tests focused on arrange/act/assert.
3. Keep setup in helpers/data providers when repeated.
4. Use consistent response validation helpers for fast-fail behavior.
5. Add traits/categories aligned with existing conventions in that project.

Detailed patterns: `references/api-automation.md`.

## UI/mobile automation workflow
1. Start from feature/scenario behavior.
2. Keep step definitions orchestration-only.
3. Add/extend page objects for UI interactions and locators.
4. Reuse base page primitives (wait/scroll/tap) instead of ad-hoc logic.
5. Keep scenario metadata/traits aligned with existing suite conventions.
6. Keep browser-driver namespaces, selectors, DOM evaluation and locator calls inside page/component
   objects or browser infrastructure. Direct xUnit tests follow this rule even when no BDD layer exists.
7. Put reusable multi-page orchestration in `Flows/` and cross-page validation in `Assertions/`.

Detailed patterns: `references/ui-automation.md`.

## Definition of done
Before finishing:

- Scope is limited to relevant project(s).
- Build succeeds (or reason for skip documented).
- Targeted tests executed (or reason for skip documented).
- Docs updated when execution/data/pipeline behavior changes.
- PR description includes impacted suites and exact commands run.

## Related docs
- `VAA.EAPI.Automation/CONTRIBUTING.md`
- `VAA.EAPI.Automation/docs/LOCAL_SETUP.md`
- `VAA.EAPI.Automation/docs/TEST_EXECUTION.md`
- `VAA.EAPI.Automation/docs/TEST_DATA_STRATEGY.md`
- `VAA.EAPI.Automation/docs/TROUBLESHOOTING.md`
- `VAA.EAPI.Automation/docs/CI_PIPELINE_MAP.md`

## Packaging note
Package the skill locally into a `.skill` artifact when needed, but do not commit packaged binaries into the repo.
