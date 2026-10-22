namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public sealed class AnchoredComponentComparer
{
    public IReadOnlyList<VisualComparisonResult> CompareHorizontalAnchorsAndCenter(
        byte[] baselinePng,
        int baselineWidth,
        int baselineHeight,
        byte[] actualPng,
        int actualWidth,
        int actualHeight,
        int leftAnchorWidth,
        int centerAnchorWidth,
        int rightAnchorWidth,
        int comparisonHeight,
        string baselineDescription,
        string artifactDirectory,
        string artifactName,
        VisualComparisonOptions options)
    {
        IReadOnlyList<VisualComparisonResult> edgeResults = CompareHorizontalAnchors(
            baselinePng,
            baselineWidth,
            baselineHeight,
            actualPng,
            actualWidth,
            actualHeight,
            leftAnchorWidth,
            rightAnchorWidth,
            comparisonHeight,
            baselineDescription,
            artifactDirectory,
            artifactName,
            options);

        if (centerAnchorWidth <= 0 || centerAnchorWidth > baselineWidth ||
            centerAnchorWidth > actualWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(centerAnchorWidth),
                "The visual centre anchor must fit within both source images.");
        }

        AppiumComponentScreenshot cropper = new();
        byte[] baselineCenter = cropper.Crop(
            baselinePng,
            baselineWidth,
            baselineHeight,
            componentX: (baselineWidth - centerAnchorWidth) / 2,
            componentY: 0,
            componentWidth: centerAnchorWidth,
            componentHeight: comparisonHeight);
        byte[] actualCenter = cropper.Crop(
            actualPng,
            actualWidth,
            actualHeight,
            componentX: (actualWidth - centerAnchorWidth) / 2,
            componentY: 0,
            componentWidth: centerAnchorWidth,
            componentHeight: comparisonHeight);
        VisualComparisonResult centerResult = new VisualImageComparer().Compare(
            baselineCenter,
            $"{baselineDescription} centre anchor",
            actualCenter,
            artifactDirectory,
            $"{artifactName}-centre",
            options);

        return [.. edgeResults, centerResult];
    }

    public IReadOnlyList<VisualComparisonResult> CompareHorizontalAnchors(
        byte[] baselinePng,
        int baselineWidth,
        int baselineHeight,
        byte[] actualPng,
        int actualWidth,
        int actualHeight,
        int leftAnchorWidth,
        int rightAnchorWidth,
        int comparisonHeight,
        string baselineDescription,
        string artifactDirectory,
        string artifactName,
        VisualComparisonOptions options)
    {
        ValidateRegion(baselineWidth, baselineHeight, leftAnchorWidth, rightAnchorWidth, comparisonHeight);
        ValidateRegion(actualWidth, actualHeight, leftAnchorWidth, rightAnchorWidth, comparisonHeight);

        AppiumComponentScreenshot cropper = new();
        VisualImageComparer comparer = new();
        byte[] baselineLeft = cropper.Crop(
            baselinePng,
            baselineWidth,
            baselineHeight,
            componentX: 0,
            componentY: 0,
            componentWidth: leftAnchorWidth,
            componentHeight: comparisonHeight);
        byte[] actualLeft = cropper.Crop(
            actualPng,
            actualWidth,
            actualHeight,
            componentX: 0,
            componentY: 0,
            componentWidth: leftAnchorWidth,
            componentHeight: comparisonHeight);
        byte[] baselineRight = cropper.Crop(
            baselinePng,
            baselineWidth,
            baselineHeight,
            componentX: baselineWidth - rightAnchorWidth,
            componentY: 0,
            componentWidth: rightAnchorWidth,
            componentHeight: comparisonHeight);
        byte[] actualRight = cropper.Crop(
            actualPng,
            actualWidth,
            actualHeight,
            componentX: actualWidth - rightAnchorWidth,
            componentY: 0,
            componentWidth: rightAnchorWidth,
            componentHeight: comparisonHeight);

        return
        [
            comparer.Compare(
                baselineLeft,
                $"{baselineDescription} left anchor",
                actualLeft,
                artifactDirectory,
                $"{artifactName}-left",
                options),
            comparer.Compare(
                baselineRight,
                $"{baselineDescription} right anchor",
                actualRight,
                artifactDirectory,
                $"{artifactName}-right",
                options),
        ];
    }

    private static void ValidateRegion(
        int width,
        int height,
        int leftAnchorWidth,
        int rightAnchorWidth,
        int comparisonHeight)
    {
        if (leftAnchorWidth <= 0 || rightAnchorWidth <= 0 || comparisonHeight <= 0 ||
            leftAnchorWidth + rightAnchorWidth > width || comparisonHeight > height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(leftAnchorWidth),
                "Visual anchor regions must fit within their source image without overlapping.");
        }
    }
}
