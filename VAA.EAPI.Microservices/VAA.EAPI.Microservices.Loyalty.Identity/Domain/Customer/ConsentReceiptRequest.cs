namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer
{
    public class ConsentReceiptRequest
    {
        public string LoyaltyMemberNumber { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public string DateOfBirth { get; set; }
        public string CountryCode { get; set; }
        public string Emailid { get; set; }
        public string MarketingPreference { get; set; }
    }
}
