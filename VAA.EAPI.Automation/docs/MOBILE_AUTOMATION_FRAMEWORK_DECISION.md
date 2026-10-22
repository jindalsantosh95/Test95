# Mobile Automation Framework Decision

For a concise, meeting-oriented summary of the harness, framework rationale and proposed
agent-assisted delivery loop, see
[`MOBILE_AUTOMATION_HARNESS_AND_AGENT_DELIVERY.md`](MOBILE_AUTOMATION_HARNESS_AND_AGENT_DELIVERY.md).

## Executive summary

Native mobile Search and Book automation will be developed within
`VAA.EAPI.Automation` using .NET, xUnit, Appium and BrowserStack.

This is not primarily a language decision. The established mobile repository is currently
more mature because it has received substantially more development. The reason for choosing
EAPI Automation is its existing backend capability, established harness and proximity to
the application contracts that drive Search and Book, Trips, OCI and related journeys.

The target operating model is:

> API arrangement → native UI execution → API verification → customer-visible UI verification

The current implementation establishes the native UI harness and end-to-end journeys.
Direct API arrangement and assertions are the next stage and must not be represented as
already implemented in the same test flow.

## Why EAPI Automation

EAPI Automation already contains API clients, models, environment configuration, test-data
capabilities and domain knowledge used by mobile journeys. Locating the Appium layer in the
same solution gives us a route to reuse those capabilities rather than maintaining a second
implementation in another language and repository.

Expected benefits include:

- Reuse of existing Search, basket, passenger, payment, order, Trips and OCI capabilities.
- Less duplicated authentication, configuration, test data and request/response modelling.
- API setup of eligible accounts, PNRs, trips, passengers and baskets.
- Backend verification after customer-critical native UI interactions.
- Better distinction between app, automation, test-data, environment and downstream failures.
- Standard .NET execution, reporting, dependency management and engineering conventions.
- A simpler test-authoring path where Cucumber is not a project requirement.

### Alignment with frontend delivery

The backend capabilities in EAPI are also powering the mobile frontend being developed.
This means the automation team already has detailed knowledge of the contracts, business
rules and successful flows that the native UI must consume.

We can use that existing knowledge to:

- Define mobile scenarios before every screen is complete.
- Build expected outcomes from established backend behaviour rather than infer them only
  from the rendered UI.
- Reuse known passenger, contact, terms, pricing, basket, payment and order rules.
- Select meaningful positive, negative and boundary scenarios earlier in delivery.
- Correlate frontend failures with the exact GraphQL operation, basket or downstream state.
- Work alongside frontend development using the same feature flow and acceptance context.

This is particularly valuable for XP-style delivery. As the native screen is implemented,
automation can prepare the scenario, data model and backend expectations, then add the UI
interaction as soon as the accessibility-ready control becomes available. The team does not
need to rediscover the complete journey after the frontend is finished.

The backend remains the source of domain behaviour while the mobile tests validate that the
native experience sends, receives and presents that behaviour correctly.

## Delivery-speed case

The framework does not make BrowserStack allocation, device startup, Appium commands or
backend responses inherently faster. The delivery improvement comes from reducing the work
around each scenario and reducing investigation and rerun time.

### Reusable journey foundation

The initial work establishes reusable handling for:

- BrowserStack sessions and diagnostics.
- Onboarding, guest entry and privacy handling.
- Feature flags and app restart.
- Airports, calendars and passenger selection.
- Flight and cabin selection.
- Passenger, contact and terms-and-conditions entry.
- Hosted payment, confirmation and Trips verification.

The first journey carries the largest setup cost. Later scenarios should mainly provide
different journey data, carrier choices and expected outcomes, adding screen behaviour only
when it is genuinely new.

### API-aware execution

As API composition is added, tests can arrange prerequisite state without consuming remote
device time and can verify results without relying only on the UI. This should produce
shorter, more deterministic journeys and fewer speculative reruns.

For example, the harness should be able to determine whether a booking is absent from the
Trips API or present in the API but missing from the app. That is more actionable than a
generic element timeout.

### Fewer authoring layers

The intended path is deliberately small:

> xUnit test → reusable flow/page object → Appium

Descriptive test names, traits and typed models retain scenario intent without requiring a
feature file and step-binding layer for this project.

## Integration with mobile development

Automation is developed alongside the native feature rather than added after the UI is
complete. The native source and its automation contract are reviewed before selectors or
page interactions are implemented.

This allows us to:

- Understand the intended control structure and customer flow early.
- Reuse accessibility identifiers already provided by the app.
- Identify missing, duplicated or unstable identifiers before feature completion.
- Agree selector naming with mobile developers while controls are still easy to change.
- Update automation alongside frontend redesigns.
- Avoid coordinates, generated UUIDs, structural XPath and translated display text.

Where a control has no stable identifier, an accessibility identifier can be added to the
mobile application branch as automation-readiness work. These changes should be separate,
clearly described app commits so reviewers can confirm that they expose metadata without
changing customer behaviour.

The delivery loop is:

> Developer implements UI → automation reviews source and accessibility contract → missing IDs are added → BrowserStack build is produced → automation consumes those IDs

This makes automation readiness part of feature delivery and reduces the delay between a
testable build becoming available and the first automated execution.

## Accessibility contract

The harness uses an accessibility-first interaction standard:

- App-owned accessibility IDs are the default locator.
- The same logical identifiers should be used across iOS and Android where practical.
- Names describe business purpose, not screen position or visual styling.
- Dynamic controls use predictable identifiers following the app's established convention.
- Identifiers remain stable through visual redesigns when the business control is unchanged.
- Interactive children and their visually meaningful parent containers have separate IDs when
  they represent different automation boundaries. An input ID does not replace the ID required
  on its complete outlined field, grouped control, sheet or dialog.
- A container ID is attached to the native element that owns the component's complete visible
  bounds. It must not be placed on an internal SwiftUI child whose frame contains only text.
- Coordinates and generated identifiers are not permanent selectors.
- Text, predicate or structural fallbacks are temporary, documented and isolated in page objects.
- Every temporary fallback records the required app change and its removal condition.

The mobile application's established naming convention remains authoritative; automation
must not create a competing selector scheme.

### Visual automation-readiness contract

Functional readiness answers “can Appium operate and observe this control?” Visual readiness
also answers “can Appium identify the exact component represented by the approved design?” A
feature intended for Figma comparison therefore requires:

- a stable ID on each complete screen, sheet, dialog or grouped component selected as a visual
  comparison boundary;
- separate stable IDs on actionable children such as fields, toggles and buttons;
- the container ID to expose bounds covering the complete visible design component;
- a mapping between Figma node, app-owned ID and intended UI state in the automation contract;
- an Appium inspector check on the development build before its PR is considered automation
  ready; and
- visual states with dynamic content to expose stable subcomponents, rather than encouraging
  full-screen coordinate crops or masking unrelated data.

If this contract is absent, automation records an explicit app-readiness blocker. It must not
substitute fixed coordinates, hierarchy selectors or an artificially relaxed image threshold.
The Search Panel visual pack originally identified three examples: Journey Type exposed only the
selected pill, Discount Code exposed only its inner input, and the Passenger selector had no ID on
the complete sheet. Development subsequently added the app-owned `JourneyTypePicker`,
`DiscountCodeFieldContainer` and `PassengerSelectorContainer` boundaries. Those IDs now resolve in
Appium and TC03, TC08 and TC10 execute their complete Figma component contracts. This progression
is the intended readiness loop: record the blocker, add the app-owned boundary with development,
then remove the fallback or blocker only after Appium verifies the runtime contract.

### Calendar interaction contract

Scrollable native calendars require more than finding a day element. On iOS, SwiftUI may
expose a lazy calendar child as displayed before it is inside the safe tappable viewport, and
the element's readable label follows the device locale. Authors and coding agents must:

- identify a date by the app-owned ISO accessibility ID, never a translated date label;
- scope gestures to the app-owned calendar scroll container rather than the whole screen;
- derive a bounded attempt allowance from the target month instead of using a fixed swipe count;
- ensure the target is above the sticky action area before clicking; and
- verify the app-owned selected-date summary immediately after the action.

These rules are deliberately stronger than the earlier WebdriverIO reference helper. They
turn an incorrect or incomplete date interaction into a failure at the calendar boundary,
instead of allowing an agent-authored booking journey to fail later with misleading data.

## Lean framework rule

The implementation uses native .NET and existing repository capabilities first:

- C# symbols, compiler references and project boundaries for safe discovery and reuse.
- Targeted repository search and concise harness documentation for Codex context.
- Appium and BrowserStack logs, screenshots, page source and network evidence.
- Existing EAPI harness, architecture and PR verification.
- Focused mobile checks only for demonstrated recurring problems.

MCPs, Serena, Cucumber or another auxiliary service are not planned dependencies. A tool
should be considered only where a specific problem cannot be solved adequately through the
native toolchain and its measurable benefit exceeds its setup and maintenance cost.

The objective is not to create another framework around the framework.

## Android reuse

The wider booking runtime remains iOS-focused. Five Search panel scenarios now share
business logic through `IFlightSearchPanelPage`, with configured-session routing to
iOS and Android adapters. Android live qualification remains pending required app IDs.
The following remain reuse targets for the broader Android suite:

- xUnit scenario intent, traits and assertions.
- Search, passenger, payment and confirmation models.
- Business-flow orchestration.
- BrowserStack configuration, reporting and diagnostics.
- API arrangement and backend verification.
- Search and Book, confirmation and Trips business outcomes.

iOS-specific selectors, calendars, keyboards, scrolling, feature flags and restart behaviour
require Android implementations where Appium behaviour differs. The Search panel contract
is now implemented; additional contracts should only follow concrete platform needs.
Use iOS accessibility IDs and Android resource IDs with aligned logical names, rather than
placing automation tokens in Android screen-reader descriptions.

See [Search panel implementation and limits](MOBILE_ANDROID_SEARCH_PANEL.md) and the
[dated WDIO comparison](MOBILE_AUTOMATION_IMPLEMENTATION_BENEFITS.md#wdio-comparison-shared-search-panel-23-september-2026).
WDIO already supports shared pages and platform selector maps: this is an EAPI implementation
improvement, not a capability exclusive to .NET or evidence of faster execution.

Android support is complete only after execution on a real Android target or BrowserStack
session. It must not be inferred from compilation or shared code alone.

## Current scope

The initial implementation provides:

- A .NET 10 Appium/xUnit mobile project in EAPI Automation.
- BrowserStack iOS execution from Windows and macOS.
- Accessibility-first page objects and observable-state waits.
- One-way and return journeys.
- Multi-passenger scenarios.
- Virgin and partner-operated flight selection.
- Passenger/contact details and terms and conditions.
- Hosted payment, booking confirmation and Trips verification.
- BrowserStack network capture for GraphQL errors and basket-ID correlation.

The implementation is available in
[PR #1713](https://github.com/VAAD1ST/eapi/pull/1713).

## Risks and constraints

- BrowserStack capacity and remote-device startup remain constraints.
- Missing accessibility IDs create temporary implementation and maintenance cost.
- Backend or environment instability can still block end-to-end journeys.
- The mobile-specific layer is less mature today than the established mobile repository.
- API/UI composition still needs to be introduced after the initial UI foundation.
- Android requires separate runtime implementation and validation.
- Large end-to-end journeys remain slower and more expensive than focused tests.

The established mobile repository remains a useful behavioural reference. Proven concepts
should be reused where they add value, without copying layers that are not required by this
solution.

## Measuring the decision

The benefit will be demonstrated through evidence rather than assumed from the framework
choice. Track:

- Time from scenario definition to first passing BrowserStack run.
- Percentage of a new scenario delivered through existing flows and page actions.
- First-run and rerun pass rates.
- Median and 95th-percentile execution duration.
- BrowserStack device minutes per scenario and regression pack.
- Average failure-diagnosis time.
- Failures by app, automation, test data, environment and downstream service.
- Percentage of controls using stable accessibility IDs.
- Fixed sleeps and coordinate interactions, with a target of zero.
- Maintenance time following a shared app change.
- Reuse achieved between iOS and Android.

## Decision statement

EAPI Automation provides the preferred foundation because it combines a lean native Appium
layer with existing backend capabilities, diagnostics, test data and engineering conventions.
The key advantage is not C# over TypeScript. It is the ability to avoid duplicate backend
integration, lean on the backend already powering frontend development, deliver automation
alongside native implementation and build API-aware mobile tests that are shorter, more
deterministic and easier to diagnose as coverage grows.
