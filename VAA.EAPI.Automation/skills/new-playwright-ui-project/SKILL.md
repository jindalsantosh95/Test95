---
name: new-playwright-ui-project
description: Scaffold a new C# Playwright UI automation project inside VAA.EAPI.Automation with the required page-object, flow, assertion, setup, local-run, and architecture-test structure. Use for new web UI automation projects; do not use to add tests to an existing project.
---

# New Playwright UI Project

Use this skill together with the repository's `api-ui-automation` skill. Read
[`../../docs/FRAMEWORK_ARCHITECTURE.md`](../../docs/FRAMEWORK_ARCHITECTURE.md) and
[`../api-ui-automation/references/ui-automation.md`](../api-ui-automation/references/ui-automation.md)
before customising generated code.

Run from `VAA.EAPI.Automation/`:

```pwsh
pwsh -NoProfile -File skills/new-playwright-ui-project/scripts/new-playwright-ui-project.ps1 `
  -DomainName Offers `
  -Description "Offers customer journeys"
```

`DomainName` must be a bounded PascalCase name. The example creates
`VAA.EAPI.Automation.Offers.UI` and adds it to `VAA.EAPI.Automation.sln`.

The generator creates the mandatory dependency direction:

```text
Tests -> Flows -> Pages -> BasePage -> Playwright session
                  |
                  -> observable page state
Tests -> Assertions
```

It also creates safe setup and regression scripts, a gitignored local configuration example,
and a project README. It refuses to overwrite an existing project. It does not restore packages,
install browsers, run a live test, commit, push, or open a pull request.

After generation:

1. Replace the starter page intent with stable, user-facing locators and observable state.
2. Put orchestration in flows and business validation in assertion helpers.
3. Source booking, loyalty, and PNR test data from the shared storage tables.
4. Run the generated setup script only when browser installation is wanted.
5. Build the project and run `ArchitectureTests` before any live environment test.
6. Run live tests only when the required base URL and authentication are configured.
