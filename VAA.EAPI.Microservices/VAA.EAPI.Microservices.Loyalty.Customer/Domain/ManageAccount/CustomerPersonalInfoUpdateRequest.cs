namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class CustomerPersonalInfoUpdateRequest
    {
        public string CustomerId { get; set; }
        public string LoyaltyNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Channel { get; set; }
        public string Title { get; set; }
        public string MiddleName {get; set;}
        public string CreatedBy { get; set; }
    }
}
