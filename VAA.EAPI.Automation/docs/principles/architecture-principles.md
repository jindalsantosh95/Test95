# Architecture Principles
**VAA.EAPI.Automation**

---

The solution is layered so that shared framework code is reusable and stable, domain
suites stay focused, and UI concerns never leak downward. These layering rules are not
just convention — they are encoded as fitness tests in
`VAA.EAPI.Automation.ArchitectureTests` and enforced by `scripts/verify.ps1`. A change
that breaks a layer fails the build. AI agents must keep changes inside the layer that
owns them.

## Layered Structure

```
Utilities / Runners (TestRunner.Service, HttpMocker, PNRCreation, XsdValidation)
        ▲   referenced by nothing
Domain suites (Book, CRM, Trips, MyAccount, NDC, OCI, Search, Heimdall, …) + UI suites
        ▲
Core / Core.Delta / Core.UI         ← shared primitives, self-contained
```

- **Core projects depend only on Core projects.** The foundation layer is self-contained;
  it must never reference a domain suite.
- **The project-reference graph stays acyclic.** No cycles between projects.
- **UI suites are referenced only by UI or Utility projects.** Core and API layers never
  depend on the UI layer.
- **Utility/runner projects are referenced by nothing.** Tooling sits at the top.

These four rules are the current `ArchitectureTests` assertions; adding a layering rule
means adding a `[Fact]` there (see [Execution & Infrastructure](execution-infrastructure-principles.md)).

## Change Placement

Add behaviour at the lowest layer that does *not* over-share it.

- Add project-specific behaviour inside the target suite first.
- Promote a helper to `Core` only when it is genuinely reused across multiple suites —
  premature promotion couples unrelated suites to the same code.
- A change that would make `Core` reference a domain suite is wrong by construction; rethink
  the placement.
- Update [`FRAMEWORK_ARCHITECTURE.md`](../FRAMEWORK_ARCHITECTURE.md) when introducing a new
  framework primitive.

## API/GraphQL Suite Layering

Keep HTTP and serialisation details out of test bodies.

```
Test class → Fixture/setup → Request helper / endpoint wrapper → Shared helpers / data providers → Assertions in test
```

- Tests arrange data, call a request wrapper, and assert; they do not build `HttpClient`s
  or hand-roll requests (see [Integration Principles](integration-principles.md)).
- Endpoint wrappers and factories own request construction, headers, and serialisation.
- Shared lookups (storage tables, token acquisition) live in helpers/data providers.

## UI/Mobile Suite Layering

```
Feature/xUnit scenarios → Thin tests/steps → Journey flows → Page/component objects → Base page → Driver/session
```

- Step definitions are thin: they orchestrate page-object methods and hold assertions; no
  locators or XPath in steps.
- Page objects expose intent-based methods and hide locator detail; inherit from `BasePage`
  and reuse its wait/scroll/tap primitives.
- The shared BDD framework in `Core.UI/Xunit/Features/` (attributes, `FeatureFileDataAttribute`,
  `ScenarioRunner`) is used by both Appium (mobile) and Selenium (WiFi portal) suites —
  extend it there, not per-suite.
- Driver selection (local vs BrowserStack `RemoteWebDriver`) belongs in driver
  infrastructure (`BrowserFactory`), auto-detected from config — not in tests.
- BDD is optional; Page Object Model separation is mandatory. Direct xUnit UI tests follow the same
  boundaries as feature-driven suites.
- Tests and flows do not import driver namespaces, declare selectors, execute browser scripts, or
  call locator APIs. Page/component objects own those mechanics and expose intent plus observable state.
- Cross-page business assertions live in tests or `Assertions/`; page objects must not become journey runners.
- The architecture fitness suite rejects direct browser primitives in UI functional test source.

## Reuse Before New Primitives

- Search the target domain for an existing helper, fixture, page object, or wrapper before
  creating one.
- Keep shared utilities pure and reusable; keep suite-specific logic in the suite.
- New projects follow the existing naming and folder conventions (see the project inventory
  in [`README.md`](../../README.md)).

> Reference: [`FRAMEWORK_ARCHITECTURE.md`](../FRAMEWORK_ARCHITECTURE.md),
> [`HARNESS_ENGINEERING.md` § Thread 3](../HARNESS_ENGINEERING.md), [`README.md`](../../README.md).
