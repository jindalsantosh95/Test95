namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class ConfigurationDetails
    {
        public bool MembershipMilesEnabled { get; set; }
        public bool MembershipMilesSpendingEnabled { get; set; }
        public bool MembershipDiscountEnabled { get; set; }
        public bool TacticalPromoCodeDiscountEnabled { get; set; }
        public bool AffinityDiscountsEnabled { get; set; }
    }
}