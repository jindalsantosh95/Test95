# Shared Search panel tests — iOS and Android

## Next delivery: results, Summary and member selection

The companion iOS branch `feature/search-book-cross-platform-action-ids` adds Summary Change/
Upgrade actions and evidence IDs, member travelling menu actions/selected state, and per-leg
descriptor/carrier text IDs. Its canonical `docs/SearchAndBook/native-search-and-book-automation-contract.md`
contains **Cross-platform delivery: Summary and member actions**, including proposed Android
exported tags/resource-ID mapping, zero/one-based indexing, action ownership and acceptance checks.
Use that section as input to Android delivery; do not duplicate the mapping in this document.
It is a local branch change pending publication/merge, not delivered Android functionality.
The existing [iOS contract](https://github.com/VAAD1ST/vaa-mobile-ios/blob/develop/docs/SearchAndBook/native-search-and-book-automation-contract.md)
will contain the new section after merge. The mobile project's
[coverage gap register](../VAA.EAPI.Automation.MobileApp.UI/SEARCH-BOOK-COVERAGE-GAPS.md)
tracks implementation versus runtime qualification.

IDs must resolve through Appium on real actionable controls, with native selected/checked and
enabled state. Keep TalkBack labels meaningful, no test IDs or customer data in identifiers, and
keep iOS/Android selectors in their own adapters. Android code and live execution are unchanged
by this handoff; existing results below describe the panel only.

## Architecture and scope

The same scenario code runs against either platform:
`xUnit test -> shared flow / IFlightSearchPanelPage -> platform page adapter -> Appium`.

- `SearchPanelTestBase` retains the existing fixture, local-session serialization, failure evidence and BrowserStack status reporting.
- `FlightSearchPanelPageFactory` selects from the actual driver's `platformName`; unsupported/missing values fail explicitly.
- `IosSearchPanelPage` reuses `NativeFlightSearchPage` for calendar navigation, passenger mechanics, recent airports and startup.
- `AndroidSearchPanelPage` implements the same contract using centralised Android ResourceIds.
- Scenarios and shared flows contain no platform branch or native selectors.
- Platform adapters normalize native selection state into the same business API. Runtime inspection
  confirmed Android journey options and calendar days expose `checked`, not `selected`;
  both reuse the adapter's checked-state reader. Already-checked journey options are not clicked.
  iOS retains its existing selection interpretation. Do not add OS branches to shared scenarios
  or use label/coordinate fallbacks to compensate for different native state attributes.
- `RecentAirportSearchFlow` owns the latest-five/deduplication assertions for both platforms.
- Each independently runnable scenario still has its own test class/session.

This consolidates the earlier parallel Android pack; it does not port results/payment/booking to Android.
No readiness scenario submits Search. No app repository was modified.

## Living inventory

| Scenario | Platform routing | Status |
| --- | --- | --- |
| SearchPanel_TC01_SearchPanel_ValidCriteriaEnableSearch | iOS / Android | Android passed after checked-state correction; iOS revalidation pending |
| SearchPanel_TC02_JourneyType_SwitchingToReturnRequiresReturnDate | iOS / Android (intended) | Android targeted qualification passed 24 September 2026; remains skipped/outside readiness pending iOS revalidation |
| SearchBook_TC36_TravelDates_EditUpdatesSummaryAndCancelPreservesCommittedDates | iOS / Android | Former PANEL-TC03; iOS passed 29 September (50s); Android historical pass, renamed version not rerun |
| SearchBook_TC37_PassengerCounts_ConfirmPersistsAndCancelDiscardsDraft | iOS / Android | Former PANEL-TC04; focused persistence/cancellation scope, iOS passed 29 September (26s); Android historical pass 23 September, revised version not rerun |
| SearchBook_TC17_RecentAirportSearches_RetainFiveMostRecentPerDirection | iOS / Android | Android passed 23 September 2026; iOS revalidation pending |
| AndroidSearchBook_TC05_DiscountCode_ValidatesAppliesAndClearsPanelEntry | Android only | Passed 23 September 2026, including Clear |
| AndroidSearchBook_TC06_SelectionSheets_DismissalPreservesRoute | Android only | Passed 23 September 2026, including airport Back/Close |

The former Android TC01–TC04 names map to shared PANEL-TC01–TC04; Android TC07 is retired in favour
of the existing TC17. The JSON contract records the mapping. There is no second Android recent-airport test.

The two Android-specific cases are genuine UI-contract exceptions, not copies of iOS tests.
The inspected iOS promo sheet does not have Android's Clear action, and its airport sheet has no
equivalent stable Back/Close-ID contract. Do not silently omit assertions or invent iOS IDs to
make these cases appear shared.

Existing iOS TC05 remains iOS-only: it covers a one-year calendar boundary and additional passenger
rules, whereas the inspected Android implementation limits dates to 330 days. The shared near-term
criteria/date scenarios do not replace or weaken that broader coverage. Existing iOS promo TC18
checks an actual fare discount; it is not equivalent to panel-only code validation.

## Configuration and selection

Set the existing gitignored `local.settings.local.json`:
- Local: `RunMode=Local`, `Platform=iOS` or `Android`, and the corresponding `Appium` block.
- Remote: `RunMode=Remote`, matching `BrowserStack.Platform`, app and device.
- Nothing in the shared tests overwrites those settings.
- The setup script validates only the selected local platform: Android requires ServerUrl,
  DeviceName, UDID, AppPackage and AppActivity; iOS retains its existing fields. Inactive iOS
  placeholders do not block Android. Eight offline checks cover this validation. This is not
  qualification of the complete Android setup/connectivity flow: the connectivity test's welcome
  screen recognition remains iOS-focused. Use the targeted panel commands below for Android.
- Platform traits indicate which adapters a test supports; **they do not select or launch a platform**.

For Android, start the configured emulator/Appium with UiAutomator2, SDK and Java available.
The launch intent remains `--esa overrideFeatureFlags native-search-and-book-flow`.
Use the English guest app with onboarding completed. Android live and BrowserStack qualification
remain pending; use the local emulator first.

From `VAA.EAPI.Automation`, run the four active shared scenarios on the configured platform:

~~~sh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter 'Category=MobileApp-SearchPanel-Readiness' --results-directory TestResults/shared-panel --logger 'trx;LogFileName=panel.trx'
pwsh ../scripts/analyse-test-run-results.ps1 -TrxPath TestResults/shared-panel/panel.trx -SuiteName SharedSearchPanel -EnvironmentName local
~~~

For Android's six active panel scenarios (four shared plus two Android-specific):

~~~sh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj -c Release --filter 'Category=MobileApp-SearchPanel-Readiness|Category=MobileApp-SearchBook-AndroidReadiness' --results-directory TestResults/android-panel --logger 'trx;LogFileName=panel.trx'
~~~

Use a new results directory per run and analyse its exact TRX. To run the same test on both platforms,
run it twice with the corresponding settings/device; do not drive two platforms through one session.
Android-only cases reject an iOS session before performing their scenario actions.
Never run this project unfiltered: other tests can book and pay.

TC17 retains its existing FunctionalValidation category and adds the shared readiness category.
The four new shared scenarios remain outside regression, with `Readiness=AppContractRequired`.
Category/readiness metadata does not auto-skip tests. Android is still `planned` in the platform contract.

### Pending journey-switch scenario

Calendar diagnosis and correction (24 September 2026): switching one-way to return preserves
departure. The old adapter clicked it again (completing a same-day range), then clicked the
intended return (starting a new incomplete range). This was an automation state-handling defect,
not a backend failure or missing date ID. The adapter now reads the app-owned departure/return
footer resource IDs and their full-date descriptions, normalizes English US/UK date order,
selects only the required endpoint, and checks the complete draft before Next. At most three
date selections are allowed; there are no sleeps, fallback selectors or whole-test retries.
The shared scenario and iOS adapter remain unchanged.

PANEL-TC02 is in `Category=MobileApp-SearchPanel-Pending`, with `Readiness=Blocked` and an explicit
xUnit `Fact(Skip=...)` reason. It cannot execute or create a fixture/session even when selected
directly by its fully qualified name. Platform traits describe intended coverage, not qualification.
Both platforms are gated to preserve one shared scenario without OS branches or unverified iOS claims.
An offline guard checks the skip reason and exclusion from the readiness category.

This gate addresses PR #2194's review feedback. The Android date-confirmation defect is now fixed.
To restore readiness: retain all existing assertions, demonstrate a passing scenario on iOS
(Android targeted evidence is recorded below), then remove the skip,
restore the readiness category/trait, and update the JSON contract, offline guard and this inventory.

Review verification (24 September 2026): Release build passed; 52 offline/retry checks passed and
the directly selected TC02 was reported skipped (no Appium session). TRX analysis found no failures,
and scoped repository verification passed. Evidence: `TestResults/android-panel-review-20260924/review.trx`.

Fix verification (24 September 2026): TC02 was temporarily unskipped for one targeted local Android
qualification run, then its Pending skip was restored. The complete scenario passed (1/1, 3m 33s)
on Pixel 10a; no search was submitted. Evidence:
`TestResults/android-calendar-fix-20260924/tc02-corrected/tc02.trx`.
All 64 offline/retry cases passed, including 12 new date-state/format cases:
`TestResults/android-calendar-fix-20260924/offline/offline.trx`.
The earlier attempt in `tc02/tc02.trx` failed on US-vs-UK full-date formatting, corrected before
the passing run. The analyser's generic downstream classification of that timeout is not the
root cause. iOS, BrowserStack and a clean combined Android suite remain unqualified; this
calendar fix does not claim to resolve the earlier separate session collision.

## Local timing investigation — 25 September 2026

Two targeted TC02 runs passed without any performance-code or driver-setting changes:
`TestResults/android-timing-20260925/tc02.trx` (27s runner duration) and
`TestResults/android-timing-20260925-repeat/tc02.trx` (25s).
The temporary diagnostic build enabled only the existing scenario; the source Pending gate was
restored afterwards. These results do not qualify iOS or the combined suite.

Timestamped Appium HTTP logs recorded 278 commands in the first run (24.89s combined command
time): session creation 6.09s, initial source 1.17s, clicks 9.16s, root element lookups 6.92s,
and session deletion 0.17s. No individual element lookup exceeded one second. The repeat had
277 commands and 23.63s combined command time. Command sums exclude runner/client overhead.

The previous 3m 33s run has no detailed command log, so its cause remains unproven. The installed
UiAutomator2 driver's idle/selector waits are possible sources of variable delays, but these
two runs did not demonstrate 10-second lookup stalls. Do not disable idle synchronization or
shorten correctness waits based on the earlier duration alone. Capture timestamped commands
again if it recurs; repeated page lookups and unconditional passenger setup remain optimization
candidates, not proven causes of the earlier slowdown.

## Selector contracts and readiness

- Android: `AndroidSearchResourceIds` and `androidReadiness.selectors` in the JSON contract.
- iOS: `NativeSearchAccessibilityIds`; added journey-option/passenger-cancel IDs are present in the inspected iOS source.
- Common ownership/routing: `sharedSearchPanel` in the JSON contract.

The updated Android build passed traveller, promo, dismissal and recent-airport checks locally.
Journey switching now has targeted Android passing evidence; do not treat the whole panel as qualified.
Do not introduce XPath/text/coordinate fallbacks.
Native presentation/state normalization belongs in adapters; business rules and assertions belong
in the shared scenario/flow. Existing iOS fallback mechanics remain isolated in its established page.

Shared tests arrange relevant criteria without clearing app storage. Recents deliberately create
six origins/destinations to replace old history and verify five-entry retention independently.
Airport codes are reference inputs, not hard-coded customer/booking records.
Android promo QA123 remains a synthetic UI-validation input, not a valid-discount claim.

## Validation and limits

Offline checks enforce central Android IDs, shared platform traits, one class per scenario, retirement
of the duplicate recent test and platform routing. A fake recent-airport implementation exercises the
real shared flow; an incorrect six-item retention implementation must fail its assertions.

Initial Android local revalidation on 23 September 2026 completed with four passes and three failures.
iOS revalidation of the consolidated adapter remains pending.

The initial combined run was stopped after Appium reported overlapping emulator sessions and
UiAutomator2 socket/instrumentation failures. The seven definitive results above come from separate,
sequential test processes on Pixel 10a (`emulator-5554`), not that interrupted attempt.
Evidence: `TestResults/android-panel-20260923-isolated/<scenario>/panel.trx`.
The repository analyser examined those seven TRX files and reported three failures.
Its generic downstream/environment classification for TC02 is not an established root cause:
the stack is waiting for journey-type UI state, before search submission.

TC01 and TC03 find `native_search_journey_round_trip` but reject its non-clickable state.
TC02 clicks one-way, then times out waiting for `selected=true`. Local Android source uses
`Role.RadioButton`; investigate the adapter's `selected` versus radio `checked` interpretation
before requesting further app changes. No test assertions were weakened or code changed for this run.
The initial session-collision cause also remains unqualified; isolated execution avoided it.

Follow-up on the same day confirmed both journey options and calendar days expose `checked=true`
when selected, while `selected` stays false. The adapter now shares one strict checked-state reader
for those controls; no shared scenarios, iOS mechanics, IDs or assertions changed. Five offline
cases cover true/false and missing/invalid values. Build and all 70 selected offline/retry checks passed.

The three affected scenarios were rerun individually after this correction: TC01 passed (2m18s),
TC03 passed (20s), and TC02 failed later waiting for date confirmation to become enabled (47s).
Evidence: `TestResults/android-checked-20260923/live/<class>/panel.trx` and
`TestResults/android-checked-20260923/offline/offline.trx`; both were analysed with the harness.
Together with the four earlier passes, six scenarios now have passing local evidence, but this is
not a single clean seven-test run. TC02 needs further diagnosis of the date-selection/confirmation
state; the analyser's generic downstream classification is not proof of a backend issue.
No iOS rerun or BrowserStack qualification was performed, and no speed advantage is claimed.
Changed-file whitespace verification and `git diff --check` passed. The broader project gate
was started with the separately completed build skipped, then interrupted after its formatting
stage produced no further output for several minutes; do not count that gate as passed.

Consolidation verification (23 September 2026): Release build succeeded and all 49 tests selected
by `Category=MobileApp-Offline` passed. The TRX analyser reported no failures. Evidence:
`TestResults/shared-panel-contracts/offline.trx` (generated locally, not committed).
The full project verification gate still reports formatting failures in existing visual files;
this consolidation does not change those files.

Remaining work includes Android ID delivery, controlled error/retry cases, logged-in rewards,
full Android date-horizon/passenger-limit coverage, results/booking, visual comparison and
BrowserStack Android qualification. The broader harness also has existing visual-file formatting
failures; unrelated accessibility-audit work in the working tree is not part of this refactor.

## PR 2177 main integration — 25 September 2026

PR 2177 incorporates main at `0e49606279`, including merged Android PR 2194. The Android
adapter, ResourceId inventory and shared Search panel base are unchanged from main. The
merged contract gate retains platform-aware scenario mapping, keeps the six new readiness/
pending scenarios outside regression and preserves TC02's explicit pending iOS qualification.
TC17 remains the shared recent-airport regression scenario rather than a platform duplicate.

The integration builds and passes 70 offline/retry checks. Live compatibility validation was
attempted on local `Pixel_10_API_36` (`emulator-5554`), stopping after TC01 failed at startup.
Android displayed **Virgin Atlantic Dev isn't responding**; `dumpsys activity lastanr` recorded
an input-dispatch timeout in `MainActivity`, waiting 5004 ms for a focus event. The test timed
out finding `nav_book_tab`. This establishes an application-not-responding condition, not its
underlying cause and not a missing-ID diagnosis. The other five active panel cases were not
run; no live pass or BrowserStack qualification is claimed for this integration.

Evidence (local, not committed):
`TestResults/pr2177-main-integration/final-offline/offline.trx` and
`TestResults/pr2177-main-integration/android/SearchPanel_TC01_SearchPanel_ValidCriteriaEnableSearch.trx`.
Both runs use the exact-file TRX analyser. The user's source settings were left untouched;
the temporary Android output configuration was restored after the attempt.

## Isolated PR validation

The initial Android/shared-panel PR is isolated from current `main`; the separate native
accessibility-audit implementation is intentionally excluded. On this clean branch, Release build
and all 51 offline/retry checks passed, with no failures reported by the TRX analyser.
Evidence: `TestResults/android-panel-pr/final/offline.trx`. Earlier counts of 70 included
separate accessibility-audit checks in the original mixed checkout, not this PR's scope.
The extracted PR uses the existing EAPI JSON and regex helpers. Live results above were captured
before this extraction; no additional live run was performed on the isolated PR branch.
Scoped repository verification passed on the isolated branch. After staging, the follow-up
`-Base origin/main -SkipBuild` check also passed, including all 16 newly added C# files.
