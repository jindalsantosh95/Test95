namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public sealed record VisualComparisonResult(
    bool IsMatch,
    bool DimensionsMatch,
    int BaselineWidth,
    int BaselineHeight,
    int ActualWidth,
    int ActualHeight,
    int MismatchedPixels,
    int ComparedPixels,
    double MismatchRatio,
    string ExpectedImagePath,
    string ActualImagePath,
    string DifferenceImagePath)
{
    public string Describe(string baselineImagePath)
        => IsMatch
            ? $"Visual baseline matched '{baselineImagePath}' ({MismatchRatio:P3} mismatch)."
            : $"Visual baseline mismatch for '{baselineImagePath}': {MismatchedPixels} of " +
              $"{ComparedPixels} pixels differed ({MismatchRatio:P3}); dimensions match: " +
              $"{DimensionsMatch} (baseline {BaselineWidth}x{BaselineHeight}, actual " +
              $"{ActualWidth}x{ActualHeight}). Expected: '{ExpectedImagePath}'. Actual: " +
              $"'{ActualImagePath}'. Diff: '{DifferenceImagePath}'.";
}
