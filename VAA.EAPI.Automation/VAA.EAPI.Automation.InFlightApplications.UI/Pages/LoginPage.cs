using Microsoft.Playwright;

using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class LoginPage : BasePage
{
    private ILocator Identifier => Page.Locator("input[name='email']");

    private ILocator Password => Page.Locator("input[name='password']");

    private ILocator LanguageTrigger => Page.GetByRole(
        AriaRole.Button,
        new PageGetByRoleOptions { Name = "Change language", Exact = false });

    private ILocator LanguageDialog => Page.GetByRole(
        AriaRole.Dialog,
        new PageGetByRoleOptions { Name = "Language preferences", Exact = true });

    private ILocator Language => LanguageDialog.GetByRole(
        AriaRole.Combobox,
        new LocatorGetByRoleOptions { Name = "Language", Exact = true });

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form").Filter(new LocatorFilterOptions
    {
        Has = Identifier
    });

    public LoginPage(IPage page) : base(page)
    {
    }

    public async Task OpenAsync()
    {
        await Page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Identifier.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task EnterCredentialsAsync(string identifier, string password)
    {
        await Identifier.FillAsync(identifier);
        await Password.FillAsync(password);
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }

    public async Task<LanguageState> ReadLanguageStateAsync()
    {
        await OpenLanguagePreferencesAsync();

        IReadOnlyList<string> availableLanguages = (await Language
                .Locator("option")
                .AllTextContentsAsync())
            .Select(NormalizeLanguageName)
            .ToArray();

        string selectedLanguage = NormalizeLanguageName(
            await Language.Locator("option:checked").InnerTextAsync());

        return new LanguageState(
            await LanguageTrigger.IsVisibleAsync(),
            selectedLanguage,
            availableLanguages,
            await Form.InnerTextAsync());
    }

    public async Task SelectLanguageWithKeyboardAsync(string language)
    {
        await OpenLanguagePreferencesWithKeyboardAsync();

        IReadOnlyList<string> availableLanguages = (await Language
                .Locator("option")
                .AllTextContentsAsync())
            .Select(NormalizeLanguageName)
            .ToArray();

        int targetIndex = FindLanguageIndex(availableLanguages, language);

        if (targetIndex < 0)
        {
            throw new InvalidOperationException(
                $"The language '{language}' was not found in the login selector.");
        }

        string initiallySelectedLanguage = await ReadSelectedLanguageAsync();
        int initiallySelectedIndex = FindLanguageIndex(availableLanguages, initiallySelectedLanguage);

        if (initiallySelectedIndex == targetIndex)
        {
            int alternateIndex = targetIndex == 0 ? 1 : 0;
            await SelectLanguageAtIndexWithKeyboardAsync(alternateIndex);

            string alternateLanguage = availableLanguages[alternateIndex];
            string selectedLanguage = await ReadSelectedLanguageAsync();
            if (!selectedLanguage.Is(alternateLanguage))
            {
                throw new InvalidOperationException(
                    $"Keyboard selection did not change the language from '{initiallySelectedLanguage}' to '{alternateLanguage}'.");
            }
        }

        await SelectLanguageAtIndexWithKeyboardAsync(targetIndex);
    }

    private async Task SelectLanguageAtIndexWithKeyboardAsync(int targetIndex)
    {
        await FocusLanguageWithKeyboardAsync();
        await Language.PressAsync("Home");

        for (int index = 0; index < targetIndex; index++)
        {
            await Language.PressAsync("ArrowDown");
        }

        await Language.PressAsync("Enter");
    }

    private async Task FocusLanguageWithKeyboardAsync()
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            bool languageHasFocus = await Language.EvaluateAsync<bool>(
                "element => document.activeElement === element");

            if (languageHasFocus)
            {
                return;
            }

            await Page.Keyboard.PressAsync("Tab");
        }

        throw new InvalidOperationException(
            "Could not reach the language dropdown using keyboard navigation.");
    }

    private async Task<string> ReadSelectedLanguageAsync()
    {
        return NormalizeLanguageName(await Language.Locator("option:checked").InnerTextAsync());
    }

    private async Task OpenLanguagePreferencesWithKeyboardAsync()
    {
        if (await Language.IsVisibleAsync())
        {
            return;
        }

        for (int attempt = 0; attempt < 20; attempt++)
        {
            await Page.Keyboard.PressAsync("Tab");

            bool triggerHasFocus = await LanguageTrigger.EvaluateAsync<bool>(
                "element => document.activeElement === element");

            if (!triggerHasFocus)
            {
                continue;
            }

            await Page.Keyboard.PressAsync("Enter");
            await Language.WaitForAsync(
                new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return;
        }

        throw new InvalidOperationException(
            "Could not reach the language selector using keyboard navigation.");
    }

    private async Task OpenLanguagePreferencesAsync()
    {
        if (await Language.IsVisibleAsync())
        {
            return;
        }

        await LanguageTrigger.ClickAsync();
        await Language.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    private static string NormalizeLanguageName(string language) =>
        language.Trim().Is("English (GB)")
            ? "English"
            : language.Trim();

    private static int FindLanguageIndex(IReadOnlyList<string> availableLanguages, string language)
    {
        for (int index = 0; index < availableLanguages.Count; index++)
        {
            if (availableLanguages[index].Is(language))
            {
                return index;
            }
        }

        return -1;
    }

    public async Task<FormState> ReadStateAsync()
    {
        return new FormState(Page.Url, await Submit.IsEnabledAsync(), await Form.InnerTextAsync());
    }
}
