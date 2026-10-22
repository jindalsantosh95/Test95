# iOS develop impact review — 16 October 2026

Reviewed `vaa-mobile-ios` `origin/develop` at
`b4697b474473f690d618c765f86265e3f72ce84f` against the mobile automation pack on EAPI
`main`. This is a focused source-to-test review, not measured Swift line/branch coverage or
live qualification.

## Seat-family changes

| iOS delivery | User-visible contract | Automation decision |
| --- | --- | --- |
| #1311 linked lap infants | The independently assignable adult passenger card is labelled `<name> + infant`; the infant remains excluded from the assignable passenger count. | Amend TC49 to require `+ infant` on the existing `seatSelection.passenger.0` state while preserving count, selection, replacement and removal assertions. Requires a build containing #1311; live and BrowserStack qualification pending. |
| #1318 child proximity guidance | A booking with a `.children` traveller shows `seatSelection.childGuidanceSheet` once, before the first eligible seat sector. The exact guidance says children under 12 should sit next to, directly in front of, or directly behind an accompanying adult or young adult. | TC51–54 implement display/absence boundaries for Virgin, unsupported outbound with eligible inbound, all unsupported, and direct Delta. Exact copy requires app-owned text IDs. Dismissal/no-repeat still requires an ID on `Choose seats`. Live qualification pending. |
| #1318 confirmation warning | A nonempty Create Order W003 message is shown at booking scope as `NativeFlightBookingConfirmationProximityWarning`; W001/W002 do not drive this UI. | Separate confirmation scenario needs deterministic W003 data plus authorised payment. Not inferred from the pre-seat guidance scenario. |

The iOS `main` commit `61f4babd11da4c98a413b3f35ab9a5243258c1a4` has no committed
SearchAndBook Swift source under the reporting scope, so the deterministic source scanner reports
that ref as unavailable rather than 0%. Develop contains 149 scoped Swift files and remains
`NeedsReview`; after adding the focused TC49 mapping it has four reviewed representative mappings,
145 unmapped files before this manifest update (144 after it), and no current-run TRX evidence.
The documented component matrix remains 19/20 (95%), including five Partial areas and one Gap;
that percentage is mapping breadth, not develop execution or Swift coverage.
