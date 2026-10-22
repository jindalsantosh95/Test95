# Test Design Principles
**VAA.EAPI.Automation**

---

A test in this suite is a contract: it states a business expectation, exercises a real
service or UI, and fails loudly and specifically when that expectation breaks. AI agents
must design tests that are deterministic, independent, and readable, so that a failure
points at a regression in the system under test — not at the test itself.

## Tests Assert Business Intent

Each test should express what the user or consumer expects, not how the framework works.

- Name tests for the behaviour and outcome (`TC01_Booking_ReturnsSuccess`,
  `SelectFreeSeats_Regression`), consistent with the existing suite's naming.
- Use a consistent method-name shape for API/GraphQL regression tests:
  `<Area>_TC##_<JourneyOrScope>_<ExpectedBehaviour>`.
  Example: `OfferSearchGraphQl_TC04_OneWay_FlexiDateFlowReturnsExpectedSearchData`.
- Keep the `TC##` identifier stable once introduced; append clarity rather than renumbering.
- Avoid vague suffixes like `Test` / `Scenario` when the expected outcome can be stated directly.
- Keep the arrange/act/assert shape visible in the test body: source data, call the
  endpoint or page-object method, assert the outcome.
- Move repeated setup into helpers or data providers — but keep the assertion in the test
  (API) or step definition (UI), where the intent is readable.
- Assert on the meaningful field, not just an HTTP status. A `200` is necessary, not
  sufficient (see [Integration Principles](integration-principles.md) on GraphQL responses).
- Include enough context in failure messages to diagnose without re-running — the PNR,
  the env, the offending path.

## Determinism

A test must give the same verdict on the same code against the same environment.

- Do not depend on wall-clock time, ambient ordering, or data another test mutated.
- Source booking-shaped data from the shared storage tables so a test uses fresh, valid
  data rather than a record that has since been cancelled or consumed — see
  [Test Data Principles](test-data-principles.md).
- Never assert on incidental ordering of arrays the service does not guarantee; sort or
  match by key first.
- When a row/fixture is missing, surface it as an explicit failure (the storage helpers
  return an `"error"` entry — `Assert.Fail` on it) rather than letting it null-deref later.
- Treat a test that passes and fails intermittently as a defect: quarantine it with a
  `Trait`, raise it, and fix the root cause. Do not "fix" flakiness with blind retries or
  `Thread.Sleep`.

## Independence And Isolation

Tests must run in any order, alone or in parallel, without interfering.

- Do not share mutable state between tests; each test arranges its own data.
- Do not rely on a test earlier in the file having run first.
- Assume parallel execution: the Heimdall runner can execute suites in parallel
  (see [Execution & Infrastructure](execution-infrastructure-principles.md)), so shared
  static state, fixed file paths, or a single reused account are hazards.
- Clean up anything a test creates that would otherwise leak into another run.

## Waits, Not Sleeps (UI/Mobile)

UI stability comes from waiting for a condition, never for a duration.

- Use the base-page wait/scroll/tap primitives in `Core.UI`; do not add `Thread.Sleep`.
- Wait for the element/state the next step needs, with an explicit timeout.
- Keep retry/stabilisation logic in the base page, not scattered through steps.

## Native Mobile Locator And Interaction Rules

- Use a stable accessibility ID supplied by the app as the primary locator for every
  interactive native control. Keep the ID in its page object or reusable handler.
- Wait for the condition required by the next action, such as an element becoming
  displayed and enabled or the next screen becoming visible. Do not use fixed delays to
  wait for navigation, animation, carousel movement, or network completion.
- Do not use screen coordinates for normal interaction. Coordinate taps are device- and
  layout-dependent and bypass the accessibility contract.
- XPath, label/text matching, and coordinate interaction are temporary fallbacks only.
  Any exception must explain why an accessibility ID is unavailable, link to the app
  change required, state its removal condition, and be isolated inside a page object.
- When iOS exposes duplicate carousel elements, select the displayed, enabled element
  whose bounds intersect the current viewport, then click it by accessibility ID.
- Shared startup interruptions and onboarding belong in reusable handlers; journey tests
  must not duplicate `Get started`, consent, sign-in bypass, or feature-flag logic.

## Traits And Categories

Traits are how tests are selected for execution and reporting — treat them as an API.

- Tag new tests with the trait/category names already used by the project under test
  (`Category=BDD`, `Category=WifiPortalBDD`, `Category=SelectFreeSeats-Regression`, …).
- The Heimdall runner and pipelines filter by assembly and trait, so an untagged or
  mis-tagged test silently never runs. Match the existing convention exactly.
- Reuse the `tag` trait pattern the runner aggregates for reporting; do not invent a
  parallel scheme.

## Readability Conventions

The codebase's style rules are enforced on new files by `verify.ps1`.

- Explicit types, not `var`; single-line `//` comments rather than `#region` for short
  blocks; `source.Is(value)` for case-insensitive compares.
- These and the full list live in [`AGENTS.md` § Design conventions](../../AGENTS.md#design-conventions);
  the `.editorconfig` rules make a newly added `.cs` file fail the format check otherwise.

> Reference: [`AGENTS.md`](../../AGENTS.md), [`FRAMEWORK_ARCHITECTURE.md`](../FRAMEWORK_ARCHITECTURE.md),
> [`skills/api-ui-automation`](../../skills/api-ui-automation/SKILL.md).
