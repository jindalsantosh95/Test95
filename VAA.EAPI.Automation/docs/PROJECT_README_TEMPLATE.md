# Project README Template (Automation)

Copy this template into project-level `README.md` files for suites under `VAA.EAPI.Automation`.

---

# {Project Name}

## Purpose

Describe the automation scope (API/GraphQL/SOAP/UI/mobile), business domain, and the quality goals this suite validates.

## Project Path

- `<relative project path>/<ProjectName>.csproj`

## Dependencies

- Shared framework/core libraries used.
- External services required by tests.
- Required local settings keys (non-secret names only).

## UI Architecture (UI projects only)

- `Tests/` or `Features/`: scenario intent and outcomes.
- `Flows/`: reusable multi-page orchestration.
- `Pages/`: page/component objects containing all selectors and browser interaction.
- `Assertions/`: cross-page business validation.
- `Infrastructure/`: browser/session configuration and reporting.

All browser-backed page objects inherit from the project `BasePage`. Functional tests and flows do
not call Playwright, Selenium or Appium driver/locator APIs directly. Remove this section only for a
non-UI automation project.

## How to Run Locally

```bash
cd VAA.EAPI.Automation
dotnet test <relative project path>/<ProjectName>.csproj -c Release
```

Trait/category example:

```bash
dotnet test <relative project path>/<ProjectName>.csproj -c Release --filter "Category=<TraitName>"
```

## Typical Trait/Category Filters

- `Category=<value1>`
- `Category=<value2>`

## Test Data Notes

- Source and setup pattern for prerequisite data.
- Cleanup expectations for created data.

## Troubleshooting Notes

- Most common project-specific failures and quick fixes.

## Ownership

- Team: `<team name>`
- Tech lead / contact: `<name or channel>`

---

When this README is created/updated, also update:

- `docs/CI_PIPELINE_MAP.md` (pipeline ownership and filters)
- `docs/TEST_DATA_STRATEGY.md` (if new data patterns are introduced)
