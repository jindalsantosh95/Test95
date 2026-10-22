namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public sealed record VisualComparisonOptions(byte ChannelTolerance, double MaximumMismatchRatio)
{
    public const byte MaximumReviewedChannelTolerance = 24;
    public const double MaximumReviewedMismatchRatio = 0.11;

    public static VisualComparisonOptions Default { get; } = new(8, 0.001);

    public void Validate()
    {
        if (ChannelTolerance > MaximumReviewedChannelTolerance)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ChannelTolerance),
                $"The channel tolerance cannot exceed the reviewed limit of {MaximumReviewedChannelTolerance}.");
        }

        if (double.IsNaN(MaximumMismatchRatio) ||
            MaximumMismatchRatio is < 0 or > MaximumReviewedMismatchRatio)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumMismatchRatio),
                $"The maximum mismatch ratio must be between zero and the reviewed limit of " +
                $"{MaximumReviewedMismatchRatio:P0}.");
        }
    }
}
