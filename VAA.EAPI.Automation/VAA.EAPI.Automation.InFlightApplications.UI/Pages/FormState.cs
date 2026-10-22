namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed record FormState(
    string Url,
    bool IsSubmitEnabled,
    string VisibleText,
    string? SelectedGenderLabel = null);
