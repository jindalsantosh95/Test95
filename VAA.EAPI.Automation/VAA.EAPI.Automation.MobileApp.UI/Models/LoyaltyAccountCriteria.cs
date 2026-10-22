namespace VAA.EAPI.Automation.MobileApp.UI.Models;

/// <param name="Environment">Azure LoyaltyAccounts partition, for example SIT or UAT.</param>
/// <param name="Tier">Flying Club tier, for example Red, Silver or Gold.</param>
/// <param name="MinimumPoints">Inclusive minimum points balance.</param>
/// <param name="MaximumPoints">Optional exclusive maximum points balance.</param>
/// <param name="UnderSixteen">Whether the account must belong to a child member.</param>
public sealed record LoyaltyAccountCriteria(
    string Environment,
    string Tier,
    int MinimumPoints = 0,
    int? MaximumPoints = null,
    bool UnderSixteen = false)
{
    public static LoyaltyAccountCriteria WithExactPoints(
        string environment,
        string tier,
        int points,
        bool underSixteen = false)
    {
        if (points == int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(points), "Exact points must be below Int32.MaxValue.");
        }

        return new LoyaltyAccountCriteria(environment, tier, points, points + 1, underSixteen);
    }
}
