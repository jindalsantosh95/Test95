# Mobile Automation Agent Hardening

- Status: implemented; live TC06 validation pending
- Owner: Mobile QA / agent automation
- Created: 2026-08-26
- Scope: `VAA.EAPI.Automation.MobileApp.UI`, mobile Codex skills, and supporting docs

## Objective

Make the .NET mobile project safe and legible for coding agents that translate development
PRs or issue acceptance criteria into maintainable xUnit/Appium coverage.

## Context

The TypeScript mobile project has useful mobile-specific controls: a machine-readable UI
map, executable scenario inventory, strict ownership boundaries, setup discovery, and richer
TC06 result-refinement checks. EAPI already has the stronger cross-API harness and autonomous
delivery loop, so the useful controls should be brought into that harness without adding a
parallel Cucumber framework.

## Acceptance Criteria

- Search & Book TC01-TC06 and their test owners are represented by a checked-in contract.
- Offline tests fail for missing/duplicate scenarios, selectors, unsupported platform claims,
  or a contract entry that no longer resolves to an xUnit test.
- TC06 validates sort/filter effects and state, not only control visibility.
- Agents have an explicit workflow for impact analysis, reuse, generation, safe validation,
  and live-test authorization.
- Setup, coverage, and platform documentation identify the machine-readable contract.

## Plan

1. Add the Search & Book automation contract and offline conformance tests.
2. Harden TC06 result-refinement assertions and reusable parsing.
3. Add the mobile test-authoring skill and documentation routes.
4. Run project build, offline tests, format, and repository harness checks.

## Decisions

- 2026-08-26: Keep direct xUnit tests as the executable acceptance layer. A JSON contract
  supplies the agent-readable scenario and selector inventory that Gherkin/UI maps provide
  in the TypeScript project, without introducing another runtime translation layer.
- 2026-08-26: Treat iOS as supported and Android as planned until Android selectors and a
  passing connectivity run exist. Contract checks must reject ambiguous platform claims.

## Validation

- `dotnet build VAA.EAPI.Automation.MobileApp.UI.csproj -c Release`
- Offline contract/parser/retry-policy tests with run-scoped TRX and analyser.
- `scripts/verify.ps1 -ProjectPath ... -SkipBuild`

## Outcome

The .NET project now has a checked-in Search & Book contract, offline drift gates, an
acceptance-criteria authoring skill, landing-page connectivity readiness, and TC06 assertions
for every enabled filter type plus price, recommended, departure-time, and duration sorting.
Build, formatting, repository harness checks, and 10 offline tests pass. A live BrowserStack
TC06 run remains intentionally pending because it consumes external capacity and requires a
configured app/environment.

A local iOS connectivity run also proved Appium/WDA startup against an installed simulator
build. It exposed a `NoReset` resume state on the native Search & Book screen, which is now an
explicit recognised readiness entry rather than a false connectivity failure.

Search & Book feature state is now supplied through the same validated iOS Appium
`processArguments` contract as the TypeScript framework. The fixture no longer visits the Debug
menu or performs a feature-flag restart.
