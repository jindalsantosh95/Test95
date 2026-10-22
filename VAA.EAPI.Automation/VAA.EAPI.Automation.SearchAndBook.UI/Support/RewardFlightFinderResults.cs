namespace VAA.EAPI.Automation.SearchAndBook.UI.Support;

public sealed record RewardFlightFinderRequiredFieldsResult(
    IReadOnlyList<string> InvalidFields,
    string SelectedMonth);

public sealed record RewardFlightFinderMonthNavigationResult(
    IReadOnlyList<string> MonthOptions,
    string SelectedMonth,
    string InitialResultMonth,
    string ReturnedResultMonth,
    bool PreviousInitiallyDisabled,
    bool NextInitiallyDisabled,
    bool PreviousAfterNextDisabled,
    bool PreviousAtFinalDisabled,
    bool NextAtFinalDisabled);
