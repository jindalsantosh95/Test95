namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    public class TierInfo : EnrolCustomerDetails
    {
        public string CurrentTier { get; set; }
        public string PreviousTier { get; set; }
        public string JoinDatetime { get; set; }
        public string TierChangeDatetime { get; set; }
    }
}
