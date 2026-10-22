using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

public sealed class LoginLanguageValidationTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "LoginLanguage")]
    public async Task LoginPage_DisplaysLanguageSelector_DefaultingToEnglish()
    {
        PortalValidationFlow flow = CreateFlow();

        await using LoginLanguageValidationSession session =
            await flow.StartLoginLanguageValidationAsync();
        LanguageState state = await session.ReadLanguageStateAsync();

        PortalAssertions.LanguageSelectorDefaultsToEnglish(state);
        await session.MarkPassedAsync("The login language selector default state was validated.");
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "LoginLanguage")]
    public async Task LanguageDropdown_ContainsSupportedLanguagesOnly()
    {
        PortalValidationFlow flow = CreateFlow();

        await using LoginLanguageValidationSession session =
            await flow.StartLoginLanguageValidationAsync();
        LanguageState state = await session.ReadLanguageStateAsync();

        PortalAssertions.ContainsSupportedLanguagesOnly(state);
        await session.MarkPassedAsync("The supported login languages were validated.");
    }

    [SkippableTheory]
    [InlineData("English")]
    [InlineData("Hindi")]
    [InlineData("Korean")]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "LoginLanguage")]
    public async Task LanguageSelector_IsKeyboardAccessible(string language)
    {
        PortalValidationFlow flow = CreateFlow();

        await using LoginLanguageValidationSession session =
            await flow.StartLoginLanguageValidationAsync();
        await session.SelectLanguageWithKeyboardAsync(language);
        LanguageState state = await session.ReadLanguageStateAsync();

        PortalAssertions.LanguageSelectorSelected(state, language);
        await session.MarkPassedAsync(
            $"The login language selector was operated with the keyboard for {language}.");
    }

    private static PortalValidationFlow CreateFlow()
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        return new PortalValidationFlow(options);
    }
}
