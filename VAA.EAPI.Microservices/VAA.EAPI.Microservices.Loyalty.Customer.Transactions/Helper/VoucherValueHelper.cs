
namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper
{
    public class VoucherValueHelper
    {
        public static int GetTierBasedValue(string memberTier, string awardStatus, bool isWingsMember)
        {

            if (isWingsMember)
            {
                int wingsBase = Constants.Application.WingsAvailable;

                return awardStatus?.Trim().ToUpperInvariant() switch
                {
                    var s when s == Constants.Application.Available.ToUpperInvariant() => wingsBase,
                    var s when s == Constants.Application.PartiallyUsed.ToUpperInvariant() => wingsBase / 2,
                    _ => 0
                };
            }

            int baseValue = memberTier?.Trim().ToUpperInvariant() switch
            {
                var t when t == Constants.Application.RedTier.ToUpperInvariant() => Constants.Application.RedBaseValue,
                var t when t == Constants.Application.SilverTier.ToUpperInvariant() => Constants.Application.SilverBaseValue,
                var t when t == Constants.Application.GoldTier.ToUpperInvariant() => Constants.Application.GoldBaseValue,
                _ => 0
            };

            return awardStatus?.Trim().ToUpperInvariant() switch
            {
                var s when s == Constants.Application.Available.ToUpperInvariant() => baseValue,
                var s when s == Constants.Application.PartiallyUsed.ToUpperInvariant() => baseValue / 2,
                _ => 0
            };
        }
    }
}
