namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{

    public class Miles
    {
        public string PassengerTypeCode { get; set; }
        public string LoyaltyMemberId { get; set; }
        public LoyaltyMemberName LoyaltyMemberName { get; set; }
        public bool LoyaltyMemberTravelling { get; set; }
        public PerPassengerDiscountAmount PerPassengerDiscountAmt { get; set; }
        public int PassengerCnt { get; set; }
        public string PayWithMilesCode { get; set; }
        public string LoyaltyMemberOwnerCode { get; set; }
        public string LoyaltyProgramTierCode { get; set; }
    }
}