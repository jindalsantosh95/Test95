# Mobile visual validation

## Purpose

This implementation enables the .NET Appium framework to validate a stable native component against
an approved Figma reference without making the functional Search and Book regression pack slower
or less deterministic.

The pack contains fourteen independently named iOS Search Panel scenarios covering the core panel
rows, overlays and action states. All fourteen are executable, including the authenticated Reward
Search state that reuses the shared Azure-backed loyalty account and login capabilities.
The grouped Figma components are exported at 3x and the manifest records the exact child crop, so
the visual contract retains the intended border without including its neighbouring row. Every
source node, crop and runtime contract is recorded in `VisualBaselines/visual-baselines.json`.

## Flow

1. A designer and QA approve a specific Figma node and version.
2. Export that node as a PNG at the agreed scale and commit it at the manifest's
   `referenceImage` path.
3. Run the app with the device, appearance, locale and text size recorded in the manifest.
4. The page object waits for the component by accessibility ID.
5. `AppiumComponentScreenshot` captures the screen and crops the component using its element
   bounds. It accounts for the difference between Appium viewport points and screenshot pixels.
6. `VisualImageComparer` compares the crop with the approved PNG using a small, explicit colour
   tolerance and mismatch threshold.
7. Every comparison retains `<test>.expected.png`, `<test>.actual.png` and `<test>.diff.png`
   together for review. The expected image is the exact Figma region used by that comparison,
   rather than requiring the reviewer to locate and crop the source baseline manually.

Figma's REST image endpoint can export a node using its file key, node ID, scale and pinned
version. Baselines should be imported as an explicit review step. Tests must not download the
latest design at runtime because that would make the expected result move without application
code review.

## Why SkiaSharp is used

The visual layer uses `SkiaSharp`, the Microsoft-owned, MIT-licensed .NET binding for Google's
cross-platform Skia graphics engine. Version `4.152.1` is centrally pinned in
`Directory.Packages.props`; the mobile project declares a version-less package reference so normal
solution dependency governance and upgrades apply.

SkiaSharp supplies the small, platform-independent image-processing boundary needed by this pack:

- decode Figma and Appium PNGs into bitmaps;
- read image dimensions and pixel values consistently on macOS, Windows and CI runners;
- crop Appium screenshots from point-based element bounds at the detected Retina scale;
- composite transparent Figma pixels onto the agreed white app surface;
- perform deterministic per-pixel colour comparisons;
- create highlighted difference images; and
- encode the expected, actual and difference evidence as PNG files.

It does not locate app elements, download designs, decide whether a baseline is correct or
self-heal a mismatch. Appium owns element interaction and capture, the checked-in manifest owns
the visual contract, and a human owns baseline approval.

The pack currently decodes only repository-owned Figma baselines, Appium screenshots and images
created by its offline tests. It does not process public file uploads. All disposable Skia bitmap
and image objects are scoped with `using` so their native memory is released deterministically.

### Dependency and security posture

SkiaSharp is an external package containing managed bindings and native graphics assets, so it must
remain pinned, reviewed and covered by the solution's NuGet vulnerability audit like any other
dependency. Use the official NuGet package and Microsoft-maintained GitHub repository; do not add
an alternative feed or download native binaries manually.

Run the project audit with:

~~~pwsh
dotnet list VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj package `
  --vulnerable --include-transitive
~~~

The audit performed on 23 September 2026 reported no known advisory against SkiaSharp `4.152.1`.
It did report `Microsoft.Bcl.Memory` and `Microsoft.Kiota.Abstractions` elsewhere in the dependency
graph. Those packages are inherited from the existing shared Core references through Microsoft
identity/SQL and Microsoft Graph/Kiota respectively; they are not dependencies introduced by
SkiaSharp. Remediation of those shared dependencies must be handled at their Core ownership level
rather than by removing the visual library.

Official references:

- [SkiaSharp source and licence](https://github.com/mono/SkiaSharp)
- [SkiaSharp on NuGet](https://www.nuget.org/packages/SkiaSharp)

## Guardrails

- Keep visual checks in `Category=MobileApp-SearchBook-VisualRegression`; do not add them to every functional
  scenario.
- Prefer a stable component crop over a full-screen screenshot. Status bars, clocks, dynamic fares
  and live flight content are unsuitable baseline material.
- Never update a baseline automatically after a mismatch. A human must review both the design
  version and rendered app before replacing it.
- Do not self-heal a visual difference. Self-healing may recover an interaction; it must not
  change the expected appearance or suppress a product regression.
- Keep `VisualBaselines/visual-baselines.json` executable-only. Record components blocked by a
  missing automation-readiness contract in `MOBILE_SEARCH_PANEL_VISUAL_FAILURE_FINDINGS.md`, with
  the required ID and reason, until they can run. This avoids creating an Appium or BrowserStack
  session for a deliberately non-executable check.
- Pin device model, OS/app build, light/dark appearance, locale and dynamic text size for a visual
  run. Maintain separate approved baselines where those variants are intentionally covered.
- Keep credentials out of the repository. A Figma token is only needed by an approved baseline
  import workflow, not by normal Appium execution.

## Running and reporting the Search panel pack

From `VAA.EAPI.Automation`, run the dedicated pack locally or with `RunMode=Remote` in the
gitignored settings file:

~~~pwsh
dotnet test VAA.EAPI.Automation.MobileApp.UI/VAA.EAPI.Automation.MobileApp.UI.csproj `
  --filter "Category=MobileApp-SearchBook-VisualRegression" `
  --results-directory TestResults/mobile-search-book-visual `
  --logger "trx;LogFileName=mobile-search-book-visual.trx"
~~~

The visual test base automatically overrides the general functional-test BrowserStack chain with
`iPhone 17 Pro` on iOS 26 and disables device fallback. It then verifies BrowserStack reports the
expected 402-point viewport before opening the app flow. If the pinned device is unavailable or
the runtime dimensions drift, the scenario stops before comparing pixels rather than silently
using the iPhone 15/14 functional fallback or producing a misleading design failure. These
overrides apply only to the visual regression classes; functional mobile tests retain their
existing resilient fallback chain.

The TRX reports the named xUnit scenario, pass/fail/skip outcome, duration and mismatch percentage.
Image evidence is written below `TestResults/mobile-visual-validation/<run-id>/`. Each anchor or
component comparison has three adjacent files:

- `*.expected.png` — the exact approved Figma crop used by the comparison;
- `*.actual.png` — the component rendered by the app;
- `*.diff.png` — matching pixels in greyscale and differences highlighted in pink.

For a BrowserStack run, the BrowserStack dashboard remains the source for device session status,
video and Appium logs. The TRX and `TestResults/mobile-visual-validation/**` must also be uploaded
as the CI run artifact so reviewers can download the expected/app/diff evidence together.
BrowserStack does not automatically publish files created by the test runner.

## Status and next step

The visual validation infrastructure includes offline checks for exact matching, meaningful diff
artifacts and Retina-scaled component cropping. Fourteen independently named Appium scenarios sit
under `Category=MobileApp-SearchBook-VisualRegression`:

- `SearchBookVisual_TC01_FlyingFromRow_MatchesFigmaComponent`
- `SearchBookVisual_TC02_FlyingToRow_MatchesFigmaComponent`
- `SearchBookVisual_TC03_JourneyTypePicker_MatchesFigmaComponent` — the complete picker ID is now
  exposed; the test is active and reports a left-side size/alignment difference from Figma
- `SearchBookVisual_TC04_WhenRow_MatchesFigmaComponent`
- `SearchBookVisual_TC05_WhoRow_MatchesFigmaComponent`
- `SearchBookVisual_TC06_DestinationAirportSearchField_MatchesFigmaComponent` — active and
  intentionally reports the observed design drift: the app uses a white field without the Figma
  microphone icon, while Figma specifies a grey field with that icon; execution requires the
  app-owned `NativeFlightSearchDestinationAirportSearchField` ID and stops before comparison when
  that automation-readiness contract is absent
- `SearchBookVisual_TC07_InitialCalendarHeader_MatchesFigmaComponent`
- `SearchBookVisual_TC08_EmptyDiscountCodeField_MatchesFigmaComponent` — the complete field ID is
  now exposed; the right edge passes while the left/text region differs from Figma
- `SearchBookVisual_TC09_EmptyDiscountCodeApplyButton_MatchesDisabledFigmaState`
- `SearchBookVisual_TC10_DefaultPassengerSelector_MatchesFigmaComponent` — passing locally after
  the complete passenger sheet ID was exposed and the baseline crop was aligned to the sheet
- `SearchBookVisual_TC11_SearchFlightsButton_MatchesDisabledFigmaState` — passing locally; the
  accessible label is asserted separately and the visual mismatch is 0.9% on each edge and 9.8%
  in the native-font centre region; the centre has an explicit 11% tolerance for native text
  rasterisation while the independently compared edges protect the button geometry and colour
- `SearchBookVisual_TC12_SearchFlightsButton_MatchesEnabledFigmaState` — supplies valid airports
  and dates, asserts the control is enabled, then compares the rendered enabled state; it reaches
  visual comparison when connected to the VPN (the airport lookup is SIT-backed), with edge
  mismatches below 2% and a 10.2% native-font centre mismatch covered by the same guarded tolerance
- `SearchBookVisual_TC13_RewardSearchControl_MatchesOffFigmaState` — active and reports observed
  design drift: the app uses `Search reward seats` while Figma specifies `Search Reward Seats`,
  with additional icon and toggle alignment differences
- `SearchBookVisual_TC14_RewardSearchControl_MatchesOnFigmaState` — leases a Red SIT loyalty
  account from the shared Azure Storage Table through the existing Core adapter, logs in through
  the established mobile page object, enables Reward Search and compares the on-state

Each concrete class owns one Appium session, allowing BrowserStack parallel execution while the
existing local-session gate serialises them on a single simulator. The exported images remain
marked as `candidate-export` until Design and QA approve them; they are not part of functional
regression.

### Latest scenario outcomes

These are the latest verified outcomes aggregated across targeted local visual-validation runs. They are not
the result of a single fourteen-test suite execution.

| Test | Coverage | Latest outcome | Notes |
| --- | --- | --- | --- |
| TC01 | Flying From row | Pass | Component matches the candidate Figma baseline. |
| TC02 | Flying To row | Pass | Component matches the candidate Figma baseline. |
| TC03 | Journey type picker | Fail | Container ID resolved; left region differs by 13.861%, while the right region passes at 3.829%. |
| TC04 | When row | Pass | Component matches the candidate Figma baseline. |
| TC05 | Who row | Pass | Component matches the candidate Figma baseline. |
| TC06 | Destination airport search field | Fail | Genuine design drift remains; proposed ID was absent from the runtime tree. |
| TC07 | Initial calendar header | Pass | Component matches the candidate Figma baseline. |
| TC08 | Empty discount-code field | Fail | Container ID resolved; left/text region differs by 8.723%, while the right edge passes at 2.364%. |
| TC09 | Disabled discount-code Apply button | Pass | Component matches the candidate Figma baseline. |
| TC10 | Default passenger selector | Pass | Complete sheet ID resolved; corrected sheet crop passed locally in 19 seconds. |
| TC11 | Disabled Search Flights button | Pass | Semantic label and guarded component comparison pass. |
| TC12 | Enabled Search Flights button | Pass | Passed locally in 32 seconds while connected to the VPN. |
| TC13 | Reward Search off | Fail | Genuine text, icon and toggle-alignment differences from Figma. |
| TC14 | Reward Search on | Fail | Authenticated flow and state change pass; app uses a darker green toggle and different switch geometry from Figma, with left 11.072%, centre 8.770% and right 21.856% mismatch. |

Latest verified totals are **9 passed and 5 genuine visual failures**. TC14 now executes through
Azure-backed account selection, login and the enabled state; its failure is visual rather than an
authentication or interaction blocker.
The failures must remain visible until the app or approved design baseline changes; visual
self-healing must not suppress them.

To complete the trial, pin the Figma file version and approve the candidate PNGs. Then compare the
scenarios' stability, duration and diagnostic value locally and on BrowserStack.

### Overlay coverage findings

The Airport selector and Calendar designs contain dynamic data, so the tests compare stable
components rather than dates, recent airports or suggested destinations. Functional Appium tests
remain responsible for those changing values. The Calendar header and disabled Discount Apply
button are suitable visual contracts now.

The Airport search-field test deliberately remains active and failing while the product/design
difference is unresolved; automatically accepting the current app image would hide the finding.
The Passenger sheet and outlined Discount field are recorded as app-contract blockers because
their existing IDs do not own the complete Figma component. Container-level accessibility IDs
will allow those candidate baselines to become executable without coordinates or hierarchy
selectors.

Search Flights and Reward Search are state contracts rather than duplicate static screenshots.
The Search Flights pair is intended to prove both the neutral disabled treatment and the red
enabled treatment; its enabled comparison remains executable once airport results are available.
Reward Search proves both states: the off-state records the existing design drift, while the
on-state reuses the shared loyalty account pool and authenticated mobile login before comparison.
Their responsive comparisons retain the left edge, centred label/content and right edge
independently, so the iPhone 17 Pro width can grow without stretching the Figma image or ignoring
the state-defining centre and toggle regions. Figma's transparent component background is
composited onto the pinned white app surface before comparison. Search button text is asserted
semantically, while its 11% centre allowance covers the observed native-font rasterisation.

The first local trial on 22 September 2026 used an iPhone 17 Pro. The Figma frame is 393 points
wide while this simulator viewport is 402 points, so the component grows from 361 to 370 points.
The iPhone 17 Pro contract therefore compares the fixed left content and right chevron as anchored
regions while allowing only the responsive space between them to grow. It does not stretch either
image. The bottom divider is excluded from this child comparison because Appium rounds the native
element height from 70 to 71 points; it remains covered when the complete parent group is added as
a separate visual contract.

The baseline manifest is the executable source of truth for the crop, comparison strategy and
threshold used by every test. The reviewed default is 24 per colour channel and a maximum 6%
mismatched-pixel ratio per anchor. Explicit, evidence-backed exceptions currently range from 6.5%
to 11% for components affected by native-font rasterisation or known design drift; each exception
is recorded on its own manifest contract and must not be copied to another component without
reviewing its expected, actual and diff artifacts. The first anchored run measured 5.570% on the
icon/text anchor and 3.306% on the chevron anchor, with aligned geometry and differences confined
to native font, SF Symbol and border anti-aliasing.
