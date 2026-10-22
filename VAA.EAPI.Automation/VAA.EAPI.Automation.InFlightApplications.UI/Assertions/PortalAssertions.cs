using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Accessibility;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Assertions;

public static class PortalAssertions
{
    public static void EntryPointLoaded(EntryPointResult result, string expectedPath)
    {
        string actualPath = NormalizePath(new Uri(result.Url).AbsolutePath);
        Assert.Equal(NormalizePath(expectedPath), actualPath, ignoreCase: true);
        Assert.False(result.Title.IsNullOrWhiteSpace(), "The page title should be present.");
    }

    private static string NormalizePath(string path)
    {
        string normalizedPath = path.TrimEnd('/');
        return normalizedPath.Length == 0 ? "/" : normalizedPath;
    }

    public static void SubmitIs(FormState state, bool expectedEnabled)
    {
        Assert.Equal(expectedEnabled, state.IsSubmitEnabled);
    }

    public static void SelectedGenderIs(FormState state, string expectedLabel)
    {
        Assert.Equal(expectedLabel, state.SelectedGenderLabel);
    }

    public static void LanguageSelectorDefaultsToEnglish(LanguageState state)
    {
        Assert.True(state.IsSelectorVisible);
        Assert.Equal("English", state.SelectedLanguage);
    }

    public static void ContainsSupportedLanguagesOnly(LanguageState state)
    {
        string[] expectedLanguages = { "English", "Hindi", "Korean" };
        string[] actualLanguages = state.AvailableLanguages
            .OrderBy(language => language, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedLanguages.OrderBy(language => language, StringComparer.Ordinal),
            actualLanguages);
    }

    public static void LanguageSelectorSelected(LanguageState state, string expectedLanguage)
    {
        Assert.Equal(expectedLanguage, state.SelectedLanguage);
    }

    public static void ContainsValidation(FormState state, string expectedText)
    {
        Assert.Contains(expectedText, state.VisibleText, StringComparison.OrdinalIgnoreCase);
    }

    public static void RemainsOnLogin(FormState state)
    {
        Assert.Equal("/", new Uri(state.Url).AbsolutePath);
        Assert.Contains("Forgot password", state.VisibleText, StringComparison.OrdinalIgnoreCase);
    }

    public static void RegistrationEmailIsOpen(FormState state)
    {
        Assert.Equal("/register/email", new Uri(state.Url).AbsolutePath);
    }

    public static void Connected(JourneyResult result)
    {
        Assert.Equal("/connection", new Uri(result.Url).AbsolutePath);
        Assert.Equal("You're connected", result.Heading);
    }

    public static void RegistrationDetailsIsOpen(JourneyResult result)
    {
        Assert.Equal("/register/details", new Uri(result.Url).AbsolutePath);
        Assert.Equal("Where in the world are you?", result.Heading);
    }

    public static void HasNoSeriousOrCriticalAccessibilityViolations(AccessibilityScanResult result)
    {
        IReadOnlyList<AccessibilityViolation> violations = result.SeriousOrCriticalViolations;
        string detail = string.Join(
            Environment.NewLine,
            violations.Select(violation =>
                $"{violation.Impact}: {violation.RuleId} - {violation.Help} ({violation.HelpUrl}); " +
                $"targets: {string.Join(", ", violation.Targets)}"));

        Assert.True(
            violations.Count == 0,
            $"{result.Url} has {violations.Count} serious or critical accessibility violation(s).{Environment.NewLine}{detail}");
    }

    public static void ShortOtpIsRejected(OtpValidationResult result)
    {
        Assert.False(result.ShortCode.IsSubmitEnabled);
        Assert.Contains("Enter the full 8-digit code", result.ShortCode.VisibleText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("There was an issue with your code", result.InvalidCode.VisibleText, StringComparison.OrdinalIgnoreCase);
    }

    public static void DetailsSubmitIsDisabled(DetailsState state)
    {
        Assert.Equal("/register/details", new Uri(state.Url).AbsolutePath);
        Assert.False(state.IsSubmitEnabled);
    }

    public static void ManualAddressIsShownWithoutLookup(DetailsState state)
    {
        Assert.True(state.IsAddressLineVisible);
        Assert.True(state.IsCityVisible);
        Assert.False(state.IsFindAddressVisible);
        Assert.False(state.IsAddAddressManuallyVisible);
        Assert.False(state.IsSubmitEnabled);
    }

    public static void MobileWasResetForUs(DetailsState state)
    {
        Assert.Equal("US", state.MobileCountry);
        Assert.Empty(state.MobileNumber);
    }

    public static void ContainsDetailsValidation(DetailsState state, string expectedText)
    {
        Assert.Contains(expectedText, state.VisibleText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("/register/details", new Uri(state.Url).AbsolutePath);
    }

    public static void DefaultsToFlyingClub(PreferencesState state)
    {
        Assert.True(state.IsFlyingClubSelected);
        Assert.False(state.IsAirlineProgrammeVisible);
        Assert.True(state.IsSubmitEnabled);
    }

    public static void PartnerProgrammeIsRequired(PreferencesState beforeSelection, PreferencesState afterSelection)
    {
        Assert.True(beforeSelection.IsAirlineProgrammeVisible);
        Assert.False(beforeSelection.IsSubmitEnabled);
        Assert.Contains("membership number must be added", beforeSelection.VisibleText, StringComparison.OrdinalIgnoreCase);
        Assert.True(afterSelection.IsSubmitEnabled);
    }

    public static void DetailedAddressValidationReachedPreferences(DetailedAddressJourneyResult result)
    {
        string observedText = string.Join(Environment.NewLine, result.ObservedForms);
        Assert.Contains("Address line 1 is required", observedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Postcode is required", observedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Enter a valid UK postcode", observedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Address contains invalid characters", observedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Enter a valid US ZIP code", observedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Enter a valid mobile number", observedText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("/register/preferences", new Uri(result.Preferences.Url).AbsolutePath);
    }

    public static void PasswordsDoNotMatch(RegistrationPasswordState state)
    {
        Assert.False(state.IsSubmitEnabled);
        Assert.Contains("Passwords do not match", state.VisibleText, StringComparison.OrdinalIgnoreCase);
    }

    public static void PasswordPageHasTermsAndDefaultPreferences(RegistrationPasswordState state)
    {
        Assert.Contains("Don't send me emails or app notifications", state.VisibleText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Virgin Red", state.VisibleText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Virgin Atlantic Flying Club Terms and conditions", state.VisibleText, StringComparison.OrdinalIgnoreCase);
        Assert.False(state.IsFlyingClubMarketingOptOutSelected);
        Assert.False(state.IsVirginRedMarketingOptOutSelected);
    }

    public static void PasswordIsValid(RegistrationPasswordState state)
    {
        Assert.Equal("/register/password", new Uri(state.Url).AbsolutePath);
        Assert.True(state.IsSubmitEnabled);
        Assert.Contains("8-20 characters", state.VisibleText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Passwords must match", state.VisibleText, StringComparison.OrdinalIgnoreCase);
    }

    public static void AuthenticatedAndAccessible(AuthenticatedJourneyResult result)
    {
        Connected(result.Journey);
        foreach (AccessibilityScanResult scan in result.AccessibilityScans)
        {
            HasNoSeriousOrCriticalAccessibilityViolations(scan);
        }
    }

    public static void PasswordResetRulesAreValidated(PasswordResetJourneyResult result)
    {
        Assert.False(result.Initial.HasValidLength);
        Assert.False(result.Initial.HasNumber);
        Assert.False(result.Initial.HasUpperCase);
        Assert.False(result.Initial.HasLowerCase);
        Assert.False(result.Initial.PasswordsMatch);

        Assert.True(result.ValidPasswordOnly.HasValidLength);
        Assert.True(result.ValidPasswordOnly.HasNumber);
        Assert.True(result.ValidPasswordOnly.HasUpperCase);
        Assert.True(result.ValidPasswordOnly.HasLowerCase);
        Assert.True(result.ValidPasswordOnly.HasValidSpecialCharacters);
        Assert.True(result.ValidPasswordOnly.HasNoForbiddenSymbols);
        Assert.False(result.ValidPasswordOnly.PasswordsMatch);
        Assert.True(result.Matching.PasswordsMatch);

        Assert.True(result.InvalidPassword.HasValidLength);
        Assert.False(result.InvalidPassword.HasNumber);
        Assert.False(result.InvalidPassword.HasUpperCase);
        Assert.True(result.InvalidPassword.HasLowerCase);
        Assert.False(result.InvalidPassword.HasNoForbiddenSymbols);

        Assert.Equal("/forgotten-password/reset", new Uri(result.ReadyToSubmit.Url).AbsolutePath);
        Assert.True(result.ReadyToSubmit.PasswordsMatch);
        Assert.True(result.ReadyToSubmit.IsSubmitEnabled);
        foreach (AccessibilityScanResult scan in result.AccessibilityScans)
        {
            HasNoSeriousOrCriticalAccessibilityViolations(scan);
        }
    }
}
