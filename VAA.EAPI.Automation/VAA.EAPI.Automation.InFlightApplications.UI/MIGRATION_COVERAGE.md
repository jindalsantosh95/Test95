# In-Flight Applications migration coverage

Source: `VAAD1ST/in-flight-applications/packages/e2e-testing/src/features`.

| Source feature | Source scenarios | Playwright coverage |
| --- | ---: | --- |
| `starlink-portal.feature` | 1 | `Tests/Smoke/EntryPointTests.cs` |
| `starlink-captive-portal.feature` | 1 | `Tests/Smoke/EntryPointTests.cs` |
| `starlink-portal-a11y.feature` | 10 | `Tests/Accessibility/PortalAccessibilityTests.cs` and journey scans |
| `starlink-portal-login.feature` | 4 | `Tests/Validation/LoginValidationTests.cs` |
| `starlink-portal-login-journey.feature` | 2 | `Tests/Journeys/LoginJourneyTests.cs` |
| `starlink-portal-password-reset.feature` | 4 | `Tests/Validation/PasswordResetValidationTests.cs` |
| `starlink-portal-password-reset-journey.feature` | 1 | `Tests/Journeys/PasswordResetJourneyTests.cs` |
| `starlink-portal-signup.feature` | 18 | Registration validation test classes |
| `starlink-portal-adult-signup-journey.feature` | 3 | `Tests/Journeys/AdultRegistrationOtpTests.cs` |
| `starlink-portal-under-18-journey.feature` | 1 | `Tests/Journeys/Under18JourneyTests.cs` |
| **Total** | **45** | **45 represented scenarios / 55 reported test cases** |

Scenario outlines are represented by xUnit theories, so their example rows produce 55 independently
reported cases. Fixed-account and registration-OTP scenarios are opt-in because they mutate shared
environment state. The source adult journey ends at a valid password form. The Playwright journey
extends that coverage by submitting the final form and verifying that Flying Club enrolment reaches
the connection page.
