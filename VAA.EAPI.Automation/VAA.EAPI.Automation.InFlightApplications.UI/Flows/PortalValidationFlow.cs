using VAA.EAPI.Automation.Core.Playwright.Accessibility;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Flows;

public sealed class PortalValidationFlow
{
    private readonly PlaywrightBrowserOptions _options;

    public PortalValidationFlow(PlaywrightBrowserOptions options)
    {
        _options = options;
    }

    public async Task<EntryPointResult> LoadEntryPointAsync(string relativePath)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        PortalPage portalPage = new(session.Page);
        await portalPage.OpenAsync(relativePath);
        await portalPage.WaitUntilReadyAsync();
        EntryPointResult result = new(portalPage.CurrentUrl, await portalPage.ReadTitleAsync());
        await session.MarkPassedAsync($"The {relativePath} entry point loaded.");
        return result;
    }

    public async Task<AccessibilityScanResult> ScanAccessibilityAsync(string relativePath)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        PortalPage portalPage = new(session.Page);
        await portalPage.OpenAsync(relativePath);
        await portalPage.WaitUntilReadyAsync();
        AccessibilityScanResult result = await portalPage.ScanAccessibilityAsync();
        await session.MarkPassedAsync($"The accessibility scan completed for {relativePath}.");
        return result;
    }

    public async Task<FormState> ValidateLoginAsync(string identifier, string password, bool submit)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        LoginPage loginPage = new(session.Page);
        await loginPage.OpenAsync();
        await loginPage.EnterCredentialsAsync(identifier, password);

        if (submit)
        {
            await loginPage.SubmitAsync();
        }

        FormState result = await loginPage.ReadStateAsync();
        await session.MarkPassedAsync("The login validation state was observed.");
        return result;
    }

    public async Task<LoginLanguageValidationSession> StartLoginLanguageValidationAsync()
    {
        PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        try
        {
            LoginPage loginPage = new(session.Page);
            await loginPage.OpenAsync();
            return new LoginLanguageValidationSession(session, loginPage);
        }
        catch
        {
            await session.DisposeAsync();
            throw;
        }
    }

    public async Task<FormState> ValidatePasswordResetAsync(string? identifier)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        PasswordResetPage passwordResetPage = new(session.Page);
        await passwordResetPage.OpenAsync();

        if (!string.IsNullOrEmpty(identifier))
        {
            await passwordResetPage.EnterIdentifierAsync(identifier);
        }

        FormState result = await passwordResetPage.ReadStateAsync();
        await session.MarkPassedAsync("The password-reset validation state was observed.");
        return result;
    }

    public async Task<FormState> ValidateRegistrationProfileAsync(
        string? firstName = null,
        string? lastName = null,
        DateOnly? dateOfBirth = null,
        string? gender = null)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationProfilePage registrationPage = new(session.Page);
        await registrationPage.OpenAsync();

        if (firstName is not null && lastName is not null && dateOfBirth is not null)
        {
            await registrationPage.EnterProfileAsync(firstName, lastName, dateOfBirth.Value, gender);
        }

        FormState result = await registrationPage.ReadStateAsync();
        await session.MarkPassedAsync("The registration-profile validation state was observed.");
        return result;
    }

    public async Task<FormState> ValidateRegistrationEmailAsync(string? email)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationProfilePage registrationPage = new(session.Page);
        await registrationPage.OpenAsync();
        await registrationPage.EnterProfileAsync(
            "Test",
            "User",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            "F");
        await registrationPage.SubmitAsync();

        RegistrationEmailPage emailPage = new(session.Page);
        await emailPage.WaitUntilOpenAsync();
        if (!string.IsNullOrEmpty(email))
        {
            await emailPage.EnterEmailAsync(email);
        }

        FormState result = await emailPage.ReadStateAsync();
        await session.MarkPassedAsync("The registration-email validation state was observed.");
        return result;
    }

    public async Task<JourneyResult> CompleteUnder18JourneyAsync()
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationProfilePage registrationPage = new(session.Page);
        await registrationPage.OpenAsync();
        await registrationPage.EnterProfileAsync(
            "Alex",
            "Brown",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-10),
            "U");
        await registrationPage.SubmitAsync();

        Under18Page under18Page = new(session.Page);
        await under18Page.WaitUntilOpenAsync();
        await under18Page.ContinueAsync();

        ConnectionPage connectionPage = new(session.Page);
        JourneyResult result = await connectionPage.WaitUntilConnectedAsync();
        await session.MarkPassedAsync("The under-18 registration journey reached the connection page.");
        return result;
    }

    public async Task<JourneyResult> VerifyAdultRegistrationOtpAsync(EapiOtpCodeProvider otpCodeProvider)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationDetailsPage detailsPage = await OpenVerifiedDetailsAsync(session, otpCodeProvider);
        JourneyResult result = await detailsPage.WaitUntilOpenAsync();
        await session.MarkPassedAsync("The adult registration OTP was verified through the EAPI OTP provider.");
        return result;
    }

    public async Task<OtpValidationResult> ValidateAdultRegistrationOtpAsync()
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        OtpPage otpPage = await OpenRegistrationOtpAsync(session);
        await otpPage.EnterCodeAsync("123456");
        FormState shortCode = await otpPage.ReadStateAsync();
        await otpPage.EnterCodeAsync("99999999");
        await otpPage.SubmitAsync();
        await otpPage.WaitForInvalidCodeErrorAsync();
        FormState invalidCode = await otpPage.ReadStateAsync();
        await session.MarkPassedAsync("The registration OTP validation states were observed.");
        return new OtpValidationResult(shortCode, invalidCode);
    }

    public async Task<DetailsState> ValidateRegistrationDetailsAsync(
        EapiOtpCodeProvider otpCodeProvider,
        RegistrationDetailsScenario scenario)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationDetailsPage detailsPage = await OpenVerifiedDetailsAsync(session, otpCodeProvider);

        switch (scenario)
        {
            case RegistrationDetailsScenario.UnresolvedAddress:
                await detailsPage.CompleteUnresolvedAddressAsync();
                break;
            case RegistrationDetailsScenario.NonLookupCountry:
                await detailsPage.SelectCountryAsync("FR");
                await detailsPage.EnterMobileAsync("0612345678");
                break;
            case RegistrationDetailsScenario.CountryResetsMobile:
                await detailsPage.EnterMobileAsync("07917414088");
                await detailsPage.SelectCountryAsync("US");
                break;
            case RegistrationDetailsScenario.InvalidUkPostcode:
                await detailsPage.EnterLookupAddressAsync("241 Old Shoreham Road", "not-a-postcode");
                await detailsPage.FindAddressAsync();
                break;
            case RegistrationDetailsScenario.InvalidUsZipCode:
                await detailsPage.SelectCountryAsync("US");
                await detailsPage.EnterLookupAddressAsync("3837 Crimson Clover Ave", "ABCDE");
                await detailsPage.FindAddressAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown details scenario.");
        }

        DetailsState result = await detailsPage.ReadStateAsync();
        await session.MarkPassedAsync($"The {scenario} registration-details state was observed.");
        return result;
    }

    public async Task<PreferencesState> ValidateRegistrationPreferencesAsync(
        EapiOtpCodeProvider otpCodeProvider,
        bool choosePartner)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationPreferencesPage preferencesPage = await OpenPreferencesAsync(session, otpCodeProvider);
        if (choosePartner)
        {
            await preferencesPage.ChoosePartnerAsync();
        }

        PreferencesState result = await preferencesPage.ReadStateAsync();
        await session.MarkPassedAsync("The registration-preferences state was observed.");
        return result;
    }

    public async Task<DetailedAddressJourneyResult> ValidateDetailedAddressJourneyAsync(
        EapiOtpCodeProvider otpCodeProvider)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationDetailsPage detailsPage = await OpenVerifiedDetailsAsync(session, otpCodeProvider);
        List<string> observedForms = [];

        await detailsPage.FindAddressAsync();
        await detailsPage.WaitForValidationAsync("Address line 1 is required");
        observedForms.Add((await detailsPage.ReadStateAsync()).VisibleText);

        await detailsPage.CompleteUnresolvedAddressAsync();
        observedForms.Add((await detailsPage.ReadStateAsync()).VisibleText);
        await detailsPage.EnterLookupAddressAsync("$£%*&@^", "not-a-postcode");
        await detailsPage.FindAddressAsync();
        await detailsPage.WaitForValidationAsync("Enter a valid UK postcode");
        observedForms.Add((await detailsPage.ReadStateAsync()).VisibleText);

        await detailsPage.SelectCountryAsync("US");
        await detailsPage.EnterLookupAddressAsync("3837 Crimson Clover Ave", "ABCDE");
        await detailsPage.FindAddressAsync();
        await detailsPage.WaitForValidationAsync("Enter a valid US ZIP code");
        observedForms.Add((await detailsPage.ReadStateAsync()).VisibleText);

        await detailsPage.SelectCountryAsync("FR");
        foreach (string invalidMobile in new[] { "andy", "06!123*5678", "06", "061234567890123456" })
        {
            await detailsPage.EnterMobileAsync(invalidMobile);
            observedForms.Add((await detailsPage.ReadStateAsync()).VisibleText);
        }

        await detailsPage.EnterMobileAsync("06 12 34 56 78");
        await detailsPage.EnterManualAddressAsync(
            "10 Rue de Rivoli",
            "Paris123",
            "Ile-de-France123",
            "75001");
        observedForms.Add((await detailsPage.ReadStateAsync()).VisibleText);
        await detailsPage.EnterManualAddressAsync(
            "10 Rue de Rivoli",
            "Paris",
            "Ile-de-France",
            "75001");
        await detailsPage.SubmitAsync();

        RegistrationPreferencesPage preferencesPage = new(session.Page);
        await preferencesPage.WaitUntilOpenAsync();
        PreferencesState preferences = await preferencesPage.ReadStateAsync();
        await session.MarkPassedAsync("The detailed address-validation journey reached registration preferences.");
        return new DetailedAddressJourneyResult(observedForms, preferences);
    }

    public async Task<PartnerPreferenceResult> SelectPartnerProgrammeAsync(EapiOtpCodeProvider otpCodeProvider)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationPreferencesPage preferencesPage = await OpenPreferencesAsync(session, otpCodeProvider);
        await preferencesPage.ChoosePartnerAsync();
        PreferencesState beforeSelection = await preferencesPage.ReadStateAsync();
        await preferencesPage.SelectFirstAirlineProgrammeAsync();
        PreferencesState afterSelection = await preferencesPage.ReadStateAsync();
        await session.MarkPassedAsync("A partner airline programme was selected.");
        return new PartnerPreferenceResult(beforeSelection, afterSelection);
    }

    public async Task<RegistrationPasswordState> ValidateRegistrationPasswordAsync(
        EapiOtpCodeProvider otpCodeProvider,
        bool enterMismatchedPasswords)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        RegistrationPasswordPage passwordPage = await OpenRegistrationPasswordAsync(session, otpCodeProvider);
        if (enterMismatchedPasswords)
        {
            await passwordPage.EnterPasswordsAsync("JoinTest1234", "JoinTest1235");
        }

        RegistrationPasswordState result = await passwordPage.ReadStateAsync();
        await session.MarkPassedAsync("The registration-password state was observed.");
        return result;
    }

    public async Task<JourneyResult> CompleteAdultEnrolmentAsync(
        EapiOtpCodeProvider otpCodeProvider)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        string uniqueSurname = CreateUniqueTestSurname();
        RegistrationPasswordPage passwordPage = await OpenRegistrationPasswordAsync(
            session,
            otpCodeProvider,
            uniqueSurname);
        await passwordPage.EnterPasswordsAsync("JoinTest123!", "JoinTest123!");
        await passwordPage.SubmitAsync();

        ConnectionPage connectionPage = new(session.Page);
        JourneyResult result = await connectionPage.WaitUntilConnectedAsync();
        await session.MarkPassedAsync("The adult applicant completed Flying Club enrolment and reached the connection page.");
        return result;
    }

    public async Task<AccessibilityScanResult> ScanAdultRegistrationStepAsync(
        EapiOtpCodeProvider otpCodeProvider,
        AdultRegistrationStep step)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        switch (step)
        {
            case AdultRegistrationStep.Email:
                await OpenRegistrationEmailAsync(session);
                break;
            case AdultRegistrationStep.Details:
                await OpenVerifiedDetailsAsync(session, otpCodeProvider);
                break;
            case AdultRegistrationStep.Preferences:
                await OpenPreferencesAsync(session, otpCodeProvider);
                break;
            case AdultRegistrationStep.Password:
                await OpenRegistrationPasswordAsync(session, otpCodeProvider);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(step), step, "Unknown registration step.");
        }

        PortalPage portalPage = new(session.Page);
        AccessibilityScanResult result = await portalPage.ScanAccessibilityAsync();
        await session.MarkPassedAsync($"The accessibility scan completed for the {step} registration step.");
        return result;
    }

    public async Task<AuthenticatedJourneyResult> CompleteLoginJourneyAsync(
        string email,
        string password,
        bool requiresMfa,
        EapiOtpCodeProvider otpCodeProvider)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        LoginPage loginPage = new(session.Page);
        await loginPage.OpenAsync();
        await loginPage.EnterCredentialsAsync(email, password);
        DateTimeOffset? otpSnapshot = requiresMfa
            ? await otpCodeProvider.CaptureAccountCodeTimestampAsync(email)
            : null;
        await loginPage.SubmitAsync();

        List<AccessibilityScanResult> scans = [];
        if (requiresMfa)
        {
            OtpPage otpPage = new(session.Page);
            await otpPage.WaitUntilOpenAsync();
            scans.Add(await ScanCurrentPageAsync(session));
            string otp = await otpCodeProvider.GetAccountCodeAsync(email, otpSnapshot);
            await otpPage.EnterCodeAsync(otp);
            await otpPage.SubmitAsync();
        }

        ConnectionPage connectionPage = new(session.Page);
        JourneyResult journey = await connectionPage.WaitUntilConnectedAsync();
        scans.Add(await ScanCurrentPageAsync(session));
        await session.MarkPassedAsync(requiresMfa
            ? "The MFA account reached the connection page."
            : "The non-MFA account reached the connection page.");
        return new AuthenticatedJourneyResult(journey, scans);
    }

    public async Task<PasswordResetJourneyResult> ValidatePasswordResetJourneyAsync(
        string email,
        EapiOtpCodeProvider otpCodeProvider)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        PasswordResetPage resetPage = new(session.Page);
        await resetPage.OpenAsync();
        await resetPage.EnterIdentifierAsync(email);
        DateTimeOffset? otpSnapshot = await otpCodeProvider.CaptureAccountCodeTimestampAsync(email);
        await resetPage.SubmitAndWaitForOtpAsync();

        OtpPage otpPage = new(session.Page);
        await otpPage.WaitUntilOpenAsync();
        AccessibilityScanResult otpScan = await ScanCurrentPageAsync(session);
        string otp = await otpCodeProvider.GetAccountCodeAsync(email, otpSnapshot);
        await otpPage.EnterCodeAsync(otp);
        await otpPage.SubmitAsync();

        PasswordResetNewPasswordPage passwordPage = new(session.Page);
        await passwordPage.WaitUntilOpenAsync();
        AccessibilityScanResult passwordScan = await ScanCurrentPageAsync(session);
        PasswordRulesState initial = await passwordPage.ReadRulesAsync();

        await passwordPage.EnterNewPasswordAsync("Test1!ab");
        PasswordRulesState validPasswordOnly = await passwordPage.ReadRulesAsync();
        await passwordPage.EnterConfirmationAsync("Test1!ab");
        PasswordRulesState matching = await passwordPage.ReadRulesAsync();

        await passwordPage.ClearAsync();
        await passwordPage.EnterNewPasswordAsync("user@pass");
        PasswordRulesState invalidPassword = await passwordPage.ReadRulesAsync();

        await passwordPage.ClearAsync();
        await passwordPage.EnterNewPasswordAsync("Test1!ab");
        await passwordPage.EnterConfirmationAsync("Test1!ab");
        PasswordRulesState readyToSubmit = await passwordPage.ReadRulesAsync();

        await session.MarkPassedAsync("The password-reset OTP and new-password validation journey completed.");
        return new PasswordResetJourneyResult(
            initial,
            validPasswordOnly,
            matching,
            invalidPassword,
            readyToSubmit,
            [otpScan, passwordScan]);
    }

    private static async Task<AccessibilityScanResult> ScanCurrentPageAsync(PlaywrightBrowserSession session)
    {
        PortalPage portalPage = new(session.Page);
        return await portalPage.ScanAccessibilityAsync();
    }

    private static async Task<RegistrationEmailPage> OpenRegistrationEmailAsync(
        PlaywrightBrowserSession session,
        string surname = "User")
    {
        RegistrationProfilePage registrationPage = new(session.Page);
        await registrationPage.OpenAsync();
        await registrationPage.EnterProfileAsync(
            "Test",
            surname,
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
            "F");
        await registrationPage.SubmitAsync();

        RegistrationEmailPage emailPage = new(session.Page);
        await emailPage.WaitUntilOpenAsync();
        return emailPage;
    }

    private static async Task<OtpPage> OpenRegistrationOtpAsync(PlaywrightBrowserSession session)
    {
        RegistrationEmailPage emailPage = await OpenRegistrationEmailAsync(session);
        string email = $"inflight-playwright-{Guid.NewGuid():N}@example.com";
        await emailPage.EnterEmailAsync(email);
        await emailPage.SubmitAsync();
        OtpPage otpPage = new(session.Page);
        await otpPage.WaitUntilOpenAsync();
        return otpPage;
    }

    private static async Task<RegistrationDetailsPage> OpenVerifiedDetailsAsync(
        PlaywrightBrowserSession session,
        EapiOtpCodeProvider otpCodeProvider,
        string surname = "User")
    {
        RegistrationEmailPage emailPage = await OpenRegistrationEmailAsync(session, surname);
        string email = $"inflight-playwright-{Guid.NewGuid():N}@example.com";
        await emailPage.EnterEmailAsync(email);
        await emailPage.SubmitAsync();

        OtpPage otpPage = new(session.Page);
        await otpPage.WaitUntilOpenAsync();
        string otp = await otpCodeProvider.GetRegistrationCodeAsync(email);
        await otpPage.EnterCodeAsync(otp);
        await otpPage.SubmitAsync();

        RegistrationDetailsPage detailsPage = new(session.Page);
        await detailsPage.WaitUntilOpenAsync();
        return detailsPage;
    }

    private static async Task<RegistrationPreferencesPage> OpenPreferencesAsync(
        PlaywrightBrowserSession session,
        EapiOtpCodeProvider otpCodeProvider,
        string surname = "User")
    {
        RegistrationDetailsPage detailsPage = await OpenVerifiedDetailsAsync(session, otpCodeProvider, surname);
        await detailsPage.CompleteManualAddressAsync();
        await detailsPage.SubmitAsync();
        RegistrationPreferencesPage preferencesPage = new(session.Page);
        await preferencesPage.WaitUntilOpenAsync();
        return preferencesPage;
    }

    private static async Task<RegistrationPasswordPage> OpenRegistrationPasswordAsync(
        PlaywrightBrowserSession session,
        EapiOtpCodeProvider otpCodeProvider,
        string surname = "User")
    {
        RegistrationPreferencesPage preferencesPage = await OpenPreferencesAsync(session, otpCodeProvider, surname);
        await preferencesPage.SubmitAsync();
        RegistrationPasswordPage passwordPage = new(session.Page);
        await passwordPage.WaitUntilOpenAsync();
        return passwordPage;
    }

    private static string CreateUniqueTestSurname()
    {
        IEnumerable<char> suffix = Guid.NewGuid()
            .ToByteArray()
            .Take(8)
            .Select(value => (char)('a' + value % 26));
        return $"User{string.Concat(suffix)}";
    }
}

public sealed record EntryPointResult(string Url, string Title);

public sealed record OtpValidationResult(FormState ShortCode, FormState InvalidCode);

public sealed record PartnerPreferenceResult(PreferencesState BeforeSelection, PreferencesState AfterSelection);

public sealed record DetailedAddressJourneyResult(
    IReadOnlyList<string> ObservedForms,
    PreferencesState Preferences);

public sealed record AuthenticatedJourneyResult(
    JourneyResult Journey,
    IReadOnlyList<AccessibilityScanResult> AccessibilityScans);

public sealed record PasswordResetJourneyResult(
    PasswordRulesState Initial,
    PasswordRulesState ValidPasswordOnly,
    PasswordRulesState Matching,
    PasswordRulesState InvalidPassword,
    PasswordRulesState ReadyToSubmit,
    IReadOnlyList<AccessibilityScanResult> AccessibilityScans);

public enum RegistrationDetailsScenario
{
    UnresolvedAddress,
    NonLookupCountry,
    CountryResetsMobile,
    InvalidUkPostcode,
    InvalidUsZipCode,
}

public enum AdultRegistrationStep
{
    Email,
    Details,
    Preferences,
    Password,
}
