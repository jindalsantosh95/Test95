# Mobile Search Panel visual failure findings

## Purpose

This report summarises the current differences between the native iOS Search Panel and its Figma
designs. It is intended for review by iOS Development, Design and QA. The visual regression pack
currently contains 14 component/state checks: 9 pass and the 5 findings below fail.

These failures have not been hidden by widening tolerances or accepting the current app rendering
as a new baseline. Design should first confirm whether Figma or the implemented app is the intended
source of truth. Development changes are required only where Design confirms that Figma remains
authoritative.

## Validation conditions

- Platform: iOS
- Device: iPhone 17 Pro
- Simulator viewport: 402 points
- Appearance: Light
- Locale: `en-GB`
- Text size: Default
- Figma page: `Search panel final designs - iOS`
- Pixel comparison tolerance: 24 per colour channel and 6% maximum mismatched pixels per region
- Responsive handling: fixed left, centre and right regions are compared independently; images are
  not stretched to disguise differences between the 393-point Figma frame and 402-point simulator

The exported Figma baselines are still candidates pending Design approval and a pinned Figma file
version. Percentage differences are diagnostic measurements, not severity ratings.

## Findings requiring review

### 1. TC03 — Journey type segmented control

[Open Figma component](https://www.figma.com/design/efCfz7N9e4r9oGvo3ynALx/01-App_Search-Panel?node-id=761-27971)

**Expected:** The selected journey-type pill matches the size and alignment shown in Figma.

**Observed:** The app's selected pill has a different size and/or horizontal alignment. The complete
segmented-control accessibility ID resolves correctly, so this is not a selector or crop fallback.

**Evidence:** Left region mismatch is **13.861%**. The right region passes at **3.829%**.

**Review requested:** Design to confirm the intended selected-pill dimensions and alignment. iOS to
align the control if Figma is authoritative; otherwise Design should update the approved baseline.

### 2. TC06 — Destination airport search field

[Open Figma component](https://www.figma.com/design/efCfz7N9e4r9oGvo3ynALx/01-App_Search-Panel?node-id=761-28561)

**Expected:** A grey search field containing the microphone icon shown in Figma.

**Observed:** The app renders a white search field without the microphone icon.

**Automation-readiness blocker:** The app-owned
`NativeFlightSearchDestinationAirportSearchField` ID existed in iOS source but was not present in
the Appium runtime accessibility tree during the verified run. TC06 now requires that ID and stops
before visual comparison when it is absent; it does not use a structural fallback. This blocker is
separate from the colour/icon design difference and must be resolved before the comparison can run.

**Review requested:** Design/Product to confirm whether voice search remains part of the intended
experience and whether the field should be grey. iOS to address both the confirmed design decision
and runtime accessibility-ID exposure.

### 3. TC08 — Empty discount-code field

[Open Figma component](https://www.figma.com/design/efCfz7N9e4r9oGvo3ynALx/01-App_Search-Panel?node-id=761-28862)

**Expected:** Placeholder text treatment matches Figma.

**Observed:** The app renders the empty placeholder with a lighter treatment. The complete field
container ID resolves, and the field outline/right edge is within tolerance.

**Evidence:** Left/text region mismatch is **8.723%**. The right edge passes at **2.364%**.

**Review requested:** Design to confirm placeholder colour, weight and opacity. iOS to align the text
style if Figma is authoritative.

### 4. TC13 — Reward Search control, off state

[Open Figma component](https://www.figma.com/design/efCfz7N9e4r9oGvo3ynALx/01-App_Search-Panel?node-id=761-28085)

**Expected:** Label text `Search Reward Seats`, with the Figma icon and toggle alignment.

**Observed:** The app renders `Search reward seats` and differs in icon and toggle alignment.
Both the row and toggle accessibility IDs resolve correctly.

**Review requested:** Design/Content to confirm the capitalisation and intended icon/control
alignment. iOS to update the label and layout where confirmed.

### 5. TC14 — Reward Search control, on state

[Open Figma component](https://www.figma.com/design/efCfz7N9e4r9oGvo3ynALx/01-App_Search-Panel?node-id=761-28155)

**Expected:** Bright-green enabled toggle and the switch geometry shown in Figma.

**Observed:** The authenticated app renders a darker green enabled toggle with different switch
geometry. The label/icon differences recorded by TC13 remain in the enabled state.

**Evidence:** Left region mismatch is **11.072%**, centre is **8.770%**, and the toggle/right region
is **21.856%**.

**Functional result:** Azure-backed account selection, app login, navigation and enabling Reward
Search all succeeded. The test waits for Appium to report the toggle as selected before taking the
screenshot. This is therefore a visual finding, not a failed login or transient toggle animation.

**Review requested:** Design to confirm enabled colour and switch dimensions. iOS to align the
native control if Figma is authoritative.

## Requested decisions

| Finding | Design/Product decision | Development action if Figma is authoritative |
| --- | --- | --- |
| TC03 Journey type | Confirm selected-pill size and alignment | Align selected pill geometry |
| TC06 Airport search | Confirm field colour and microphone requirement | Align field/icon and expose the field ID at runtime |
| TC08 Discount code | Confirm placeholder typography and opacity | Align placeholder styling |
| TC13 Reward Search off | Confirm copy capitalisation, icon and alignment | Align copy and component layout |
| TC14 Reward Search on | Confirm enabled colour and switch geometry | Align enabled control styling |

## Resolution workflow

1. Design/Product records whether Figma or the current app is correct for each finding.
2. If Figma is correct, iOS updates the component without changing its stable accessibility IDs.
3. If the app is correct, Design updates Figma and QA refreshes the baseline only after approval.
4. QA reruns the five affected tests and attaches expected, actual and highlighted-difference PNGs.
5. Once approved, pin the Figma file version so future comparisons use a traceable design revision.
