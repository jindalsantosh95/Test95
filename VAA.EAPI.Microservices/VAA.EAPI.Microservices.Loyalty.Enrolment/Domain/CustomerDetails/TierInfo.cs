namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails
{
    public class TierInfo : Customer
    {
        public string CurrentTier { get; set; }
        public string PreviousTier { get; set; }
        public string JoinDatetime { get; set; }
        public string TierChangeDatetime { get; set; }
    }
}
