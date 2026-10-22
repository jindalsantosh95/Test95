namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed record LanguageState(
    bool IsSelectorVisible,
    string SelectedLanguage,
    IReadOnlyList<string> AvailableLanguages,
    string VisibleText);
