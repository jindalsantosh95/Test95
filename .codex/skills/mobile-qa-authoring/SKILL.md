---
name: mobile-qa-authoring
description: Create or update EAPI Appium/xUnit mobile automation from a development PR, issue, acceptance criteria, or requested mobile scenario. Use for mobile test impact analysis and implementation; use mobile-qa-setup instead for workstation setup or connectivity only.
---

# Mobile QA Authoring

Turn the supplied product change or acceptance criteria into the smallest maintainable mobile
automation change inside `VAA.EAPI.Automation.MobileApp.UI`.

## Contract First

Read these before editing:

- `VAA.EAPI.Automation.MobileApp.UI/Contracts/search-book-automation-contract.json`
- `VAA.EAPI.Automation.MobileApp.UI/README.md`
- the closest existing test, page object, accessibility-ID owner, flow, and model
- relevant EAPI backend automation when the outcome can be verified below the UI

Map each acceptance criterion to one of: existing coverage, amended coverage, new coverage,
or blocked by a missing app-owned selector/test-data contract. Report that map before broad
implementation. Search with `rg` before adding a page object, flow, model, selector, or helper;
extend the closest owner unless it has a genuinely different responsibility.

Update the JSON contract in the same change whenever scenario ownership, category, platform,
outcome, or stable selector inventory changes. The offline contract tests are the coverage and
selector drift gate.

## Implementation Boundaries

- Keep xUnit tests outcome-focused; keep Appium interaction and observable waits in page objects
  or flows. Do not add Cucumber as a second acceptance runtime.
- Give every independently runnable functional scenario its own concrete test class. This is the
  unit of BrowserStack parallelism and Appium-session isolation. Do not place multiple live
  scenarios in one class or add them to the non-parallel `Mobile` collection.
- Keep one complete user outcome together: do not split search, payment, confirmation, and Trips
  into ordered tests. Extract repeated journey mechanics into an abstract test base, flow, or page
  object while each concrete class owns one `[Fact]` and can run alone in any order.
- Rely on the harness local-session gate to serialize `RunMode=Local`; do not disable remote
  parallelism to accommodate a single simulator.
- Use app-owned accessibility identifiers from the central ID classes. If a required identifier
  is absent, document the app contract needed rather than hiding the gap behind brittle XPath,
  coordinates, or sleeps.
- For Figma or screenshot validation, require a stable ID on the native element owning the
  complete visual bounds. Keep separate IDs on actionable children. If an ID resolves only to an
  inner label/input, or a sheet/group has no container ID, mark the visual contract blocked and
  describe the app change; never infer its bounds from coordinates or relax the image threshold.
- Reuse EAPI API clients, test-data builders, correlation identifiers, and backend assertions for
  outcomes that the UI cannot prove reliably. Never infer a PNR, email, payment, or downstream
  state solely from visible text when EAPI can verify the owning system.
- Mark tests with `Category`, `TestType`, `Feature` or journey traits, and `Platform`. Do not claim
  Android support until its selectors and connectivity path pass.
- Preserve failure evidence and BrowserStack status reporting through `SearchBookTestBase`.

## Validation Tiers

Always build and run `Category=MobileApp-Offline` plus the retry-policy tests with run-scoped TRX
and the repository analyser. Run `scripts/verify.ps1` for the project scope.

The connectivity test creates an external Appium session. Search & Book functional tests consume
BrowserStack capacity, and smoke E2E tests can create bookings and payments. Run those tiers only
when the user explicitly authorizes the relevant live action and the target environment is ready.

Finish with a concise coverage map, changed contract entries, commands/evidence, and any missing
app selector, data, backend-verification, or platform contract. Do not push, update issues, or
modify a PR unless the user has authorized that external change.
