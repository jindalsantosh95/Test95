using VAA.EAPI.Automation.Core.Serialisation;

namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public sealed record VisualBaselineCrop(int X, int Y, int Width, int Height);

public sealed record VisualBaselineFigma(
    string NodeId,
    VisualBaselineCrop TargetCropPixels);

public sealed record VisualBaselineRuntime(
    string Platform,
    string Device,
    int ViewportWidthPoints,
    string Appearance,
    string Locale,
    string TextSize);

public sealed record VisualBaselineComparison(
    string Strategy,
    int LeftAnchorWidthPixels,
    int? CenterAnchorWidthPixels,
    int RightAnchorWidthPixels,
    int ComparisonHeightPixels,
    byte ChannelTolerance,
    double MaximumMismatchRatio);

public sealed record VisualBaselineContract(
    string Id,
    string Status,
    string AccessibilityId,
    string ReferenceImage,
    VisualBaselineFigma Figma,
    VisualBaselineRuntime Runtime,
    VisualBaselineComparison Comparison);

public sealed record VisualBaselineManifest(
    int SchemaVersion,
    IReadOnlyList<VisualBaselineContract> Baselines)
{
    private const string ManifestRelativePath = "VisualBaselines/visual-baselines.json";
    public static VisualBaselineManifest Load(string baseDirectory)
    {
        string manifestPath = Path.Combine(baseDirectory, ManifestRelativePath);
        string json = File.ReadAllText(manifestPath);
        VisualBaselineManifest manifest = Json.Deserialize<VisualBaselineManifest>(json)
            ?? throw new InvalidDataException($"Visual baseline manifest '{manifestPath}' was empty.");

        manifest.Validate(baseDirectory);
        return manifest;
    }

    public VisualBaselineContract Get(string id)
        => Baselines.SingleOrDefault(contract => contract.Id.Equals(id, StringComparison.Ordinal))
           ?? throw new InvalidDataException($"Visual baseline contract '{id}' was not found.");

    private void Validate(string baseDirectory)
    {
        if (SchemaVersion != 1 || Baselines.Count == 0)
        {
            throw new InvalidDataException("The visual baseline manifest schema or baseline collection is invalid.");
        }

        if (Baselines.Select(contract => contract.Id).Distinct(StringComparer.Ordinal).Count() != Baselines.Count)
        {
            throw new InvalidDataException("Visual baseline contract IDs must be unique.");
        }

        string root = Path.GetFullPath(baseDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        foreach (VisualBaselineContract contract in Baselines)
        {
            if (contract.Status is not "candidate-export" and not "design-drift-observed" and not "approved")
            {
                throw new InvalidDataException(
                    $"Executable visual baseline contract '{contract.Id}' has an unsupported status " +
                    $"'{contract.Status}'. Record blocked readiness items in the visual failure-findings " +
                    "document instead of this executable manifest.");
            }

            if (contract.AccessibilityId.StartsWith("structural:", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Visual baseline contract '{contract.Id}' must use an app-owned accessibility ID.");
            }

            VisualBaselineCrop crop = contract.Figma.TargetCropPixels;
            if (crop.X < 0 || crop.Y < 0 || crop.Width <= 0 || crop.Height <= 0)
            {
                throw new InvalidDataException($"Visual baseline contract '{contract.Id}' has an invalid crop.");
            }

            if (contract.Comparison.Strategy is not "horizontal-anchors" and not "horizontal-anchors-and-centre")
            {
                throw new InvalidDataException(
                    $"Visual baseline contract '{contract.Id}' uses an unsupported comparison strategy.");
            }

            if (contract.Comparison.Strategy == "horizontal-anchors-and-centre" &&
                contract.Comparison.CenterAnchorWidthPixels is null)
            {
                throw new InvalidDataException(
                    $"Visual baseline contract '{contract.Id}' requires a centre anchor width.");
            }

            new VisualComparisonOptions(
                contract.Comparison.ChannelTolerance,
                contract.Comparison.MaximumMismatchRatio).Validate();

            string referencePath = Path.GetFullPath(Path.Combine(baseDirectory, contract.ReferenceImage));
            if (!referencePath.StartsWith(root, StringComparison.Ordinal) || !File.Exists(referencePath))
            {
                throw new InvalidDataException(
                    $"Visual baseline contract '{contract.Id}' references an unavailable image inside the output directory.");
            }
        }
    }
}
